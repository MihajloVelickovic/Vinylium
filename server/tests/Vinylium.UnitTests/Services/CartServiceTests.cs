using app.Models;
using app.Repositories;
using app.Services;
using Moq;

namespace Vinylium.UnitTests.Services;

[TestFixture]
public class CartServiceTests{
	private const string Barcode = "5099969944123";

	private Mock<ICartRepository> _cartRepository = null!;
	private Mock<IProductService> _productService = null!;
	private Mock<IStoreService> _storeService = null!;
	private CartService _sut = null!;

	private Guid _storeId;
	private Guid _cartId;

	[SetUp]
	public void SetUp(){
		_cartRepository = new Mock<ICartRepository>();
		_productService = new Mock<IProductService>();
		_storeService = new Mock<IStoreService>();
		_sut = new CartService(_cartRepository.Object, _productService.Object, _storeService.Object);

		_storeId = Guid.CreateVersion7();
		_cartId = Guid.CreateVersion7();

		_storeService.Setup(s => s.GetStoreByIdAsync(_storeId))
			.ReturnsAsync(TestData.NewStore(_storeId));
	}

	private void ProductIs(decimal? price = 19.99m, bool inStock = true){
		_productService.Setup(p => p.GetByIdAsync(Barcode))
			.ReturnsAsync(TestData.NewProduct(Barcode, price, inStock));
	}

	private void StockIs(int quantity, Guid? storeId = null){
		_productService.Setup(p => p.GetAvailableStoresByIdAsync(Barcode))
			.ReturnsAsync([TestData.NewStock(Barcode, storeId ?? _storeId, quantity)]);
	}

	private void CartIs(Cart? cart){
		_cartRepository.Setup(c => c.GetCartAsync(_cartId)).ReturnsAsync(cart);
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void AddItem_NonPositiveQuantity_ThrowsWithoutLookingUpProduct(int quantity){
		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddItemAsync(null, Barcode, _storeId, quantity));

		Assert.That(ex!.Message, Is.EqualTo("Quantity must be greater than zero"));
		_productService.Verify(p => p.GetByIdAsync(It.IsAny<string>()), Times.Never);
	}

	[Test]
	public void AddItem_ProductOutOfStock_Throws(){
		ProductIs(inStock: false);

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddItemAsync(null, Barcode, _storeId, 1));

		Assert.That(ex!.Message, Is.EqualTo($"Product {Barcode} is out of stock"));
		_cartRepository.Verify(c => c.CreateCartAsync(), Times.Never);
	}

	[Test]
	public void AddItem_ProductWithoutPrice_Throws(){
		ProductIs(price: null);

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddItemAsync(null, Barcode, _storeId, 1));

		Assert.That(ex!.Message, Is.EqualTo($"Product {Barcode} is not available for purchase yet"));
		_cartRepository.Verify(c => c.CreateCartAsync(), Times.Never);
		_cartRepository.Verify(c => c.AddOrUpdateItemAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
			It.IsAny<int>()), Times.Never);
	}

	[Test]
	public void AddItem_StoreDoesNotCarryProduct_Throws(){
		ProductIs();
		StockIs(10, storeId: Guid.CreateVersion7());

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddItemAsync(null, Barcode, _storeId, 1));

		Assert.That(ex!.Message, Is.EqualTo("Selected store does not carry this product"));
	}

	[Test]
	public void AddItem_MoreThanStoreHas_Throws(){
		ProductIs();
		StockIs(3);

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddItemAsync(null, Barcode, _storeId, 5));

		Assert.That(ex!.Message, Is.EqualTo($"Only 3 of {Barcode} available at the selected store"));
	}

	[Test]
	public void AddItem_AlreadyInCart_ValidatesAgainstCombinedQuantity(){
		ProductIs();
		StockIs(3);
		CartIs(TestData.NewCart(_cartId, TestData.NewCartItem(Barcode, _storeId, 2, _cartId)));

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddItemAsync(_cartId, Barcode, _storeId, 2));

		Assert.That(ex!.Message, Is.EqualTo($"Only 3 of {Barcode} available at the selected store"));
		_cartRepository.Verify(c => c.AddOrUpdateItemAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
			It.IsAny<int>()), Times.Never);
	}

	[Test]
	public async Task AddItem_AlreadyInCart_AllowsExactlyTheRemainingStock(){
		ProductIs();
		StockIs(3);
		CartIs(TestData.NewCart(_cartId, TestData.NewCartItem(Barcode, _storeId, 2, _cartId)));

		await _sut.AddItemAsync(_cartId, Barcode, _storeId, 1);

		_cartRepository.Verify(c => c.AddOrUpdateItemAsync(_cartId, Barcode, _storeId, 1), Times.Once);
	}

	[Test]
	public async Task AddItem_NoCartYet_CreatesOne(){
		ProductIs();
		StockIs(5);
		var created = TestData.NewCart(_cartId, TestData.NewCartItem(Barcode, _storeId, 2, _cartId));
		_cartRepository.Setup(c => c.CreateCartAsync()).ReturnsAsync(created);
		CartIs(created);

		var view = await _sut.AddItemAsync(null, Barcode, _storeId, 2);

		_cartRepository.Verify(c => c.CreateCartAsync(), Times.Once);
		_cartRepository.Verify(c => c.AddOrUpdateItemAsync(_cartId, Barcode, _storeId, 2), Times.Once);
		Assert.That(view.Id, Is.EqualTo(_cartId));
	}

	[Test]
	public async Task AddItem_ExistingCart_ReusesIt(){
		ProductIs();
		StockIs(5);
		CartIs(TestData.NewCart(_cartId, TestData.NewCartItem(Barcode, _storeId, 1, _cartId)));

		await _sut.AddItemAsync(_cartId, Barcode, _storeId, 1);

		_cartRepository.Verify(c => c.CreateCartAsync(), Times.Never);
		_cartRepository.Verify(c => c.AddOrUpdateItemAsync(_cartId, Barcode, _storeId, 1), Times.Once);
	}

	[Test]
	public async Task AddItem_BuildsViewFromProductAndStore(){
		ProductIs(price: 24.50m);
		StockIs(5);
		CartIs(TestData.NewCart(_cartId, TestData.NewCartItem(Barcode, _storeId, 2, _cartId)));

		var view = await _sut.AddItemAsync(_cartId, Barcode, _storeId, 1);

		Assert.That(view.Items, Has.Count.EqualTo(1));
		var item = view.Items[0];
		Assert.Multiple(() => {
			Assert.That(item.Barcode, Is.EqualTo(Barcode));
			Assert.That(item.Name, Is.EqualTo("Abbey Road"));
			Assert.That(item.Artist, Is.EqualTo("The Beatles"));
			Assert.That(item.Price, Is.EqualTo(24.50m));
			Assert.That(item.Quantity, Is.EqualTo(2));
			Assert.That(item.StoreId, Is.EqualTo(_storeId));
			Assert.That(item.StoreName, Is.EqualTo("Vinylium Niš"));
		});
	}

	[Test]
	public async Task UpdateItem_ToZero_SkipsStockValidation(){
		CartIs(TestData.NewCart(_cartId));

		await _sut.UpdateItemAsync(_cartId, Barcode, _storeId, 0);

		_productService.Verify(p => p.GetAvailableStoresByIdAsync(It.IsAny<string>()), Times.Never);
		_cartRepository.Verify(c => c.SetItemQuantityAsync(_cartId, Barcode, _storeId, 0), Times.Once);
	}

	[Test]
	public void UpdateItem_AboveStoreStock_ThrowsWithoutWriting(){
		StockIs(1);

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.UpdateItemAsync(_cartId, Barcode, _storeId, 4));

		Assert.That(ex!.Message, Is.EqualTo($"Only 1 of {Barcode} available at the selected store"));
		_cartRepository.Verify(c => c.SetItemQuantityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
			It.IsAny<int>()), Times.Never);
	}

	[Test]
	public async Task UpdateItem_LastItemRemoved_DeletesCartAndReturnsNull(){
		CartIs(TestData.NewCart(_cartId));

		var view = await _sut.UpdateItemAsync(_cartId, Barcode, _storeId, 0);

		Assert.That(view, Is.Null);
		_cartRepository.Verify(c => c.DeleteCartAsync(_cartId), Times.Once);
	}

	[Test]
	public async Task UpdateItem_ItemsRemain_KeepsCartAndReturnsView(){
		ProductIs();
		StockIs(5);
		CartIs(TestData.NewCart(_cartId, TestData.NewCartItem(Barcode, _storeId, 2, _cartId)));

		var view = await _sut.UpdateItemAsync(_cartId, Barcode, _storeId, 2);

		Assert.That(view, Is.Not.Null);
		Assert.That(view!.Items, Has.Count.EqualTo(1));
		_cartRepository.Verify(c => c.DeleteCartAsync(It.IsAny<Guid>()), Times.Never);
	}

	[Test]
	public async Task RemoveItem_LastItem_DeletesCartAndReturnsNull(){
		CartIs(TestData.NewCart(_cartId));

		var view = await _sut.RemoveItemAsync(_cartId, Barcode, _storeId);

		Assert.That(view, Is.Null);
		_cartRepository.Verify(c => c.RemoveItemAsync(_cartId, Barcode, _storeId), Times.Once);
		_cartRepository.Verify(c => c.DeleteCartAsync(_cartId), Times.Once);
	}

	[Test]
	public async Task RemoveItem_OtherItemsRemain_ReturnsView(){
		ProductIs();
		CartIs(TestData.NewCart(_cartId, TestData.NewCartItem(Barcode, _storeId, 1, _cartId)));

		var view = await _sut.RemoveItemAsync(_cartId, Barcode, _storeId);

		Assert.That(view, Is.Not.Null);
		_cartRepository.Verify(c => c.DeleteCartAsync(It.IsAny<Guid>()), Times.Never);
	}

	[Test]
	public async Task GetCart_UnknownCart_ReturnsNull(){
		CartIs(null);

		Assert.That(await _sut.GetCartAsync(_cartId), Is.Null);
	}

	[Test]
	public void UpdateItem_CartGoneMidRequest_Throws(){
		CartIs(null);

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.UpdateItemAsync(_cartId, Barcode, _storeId, 0));

		Assert.That(ex!.Message, Is.EqualTo("Cart not found"));
	}

	[Test]
	public async Task DeleteCart_DelegatesToRepository(){
		await _sut.DeleteCartAsync(_cartId);

		_cartRepository.Verify(c => c.DeleteCartAsync(_cartId), Times.Once);
	}
}
