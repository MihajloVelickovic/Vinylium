using app.Enums;
using app.Models;
using app.Repositories;
using app.Requests;
using app.Services;
using Moq;
using Newtonsoft.Json.Linq;

namespace app.UnitTest.Services;

[TestFixture]
public class ProductServiceTests{
	private const string Barcode = "5099969944123";

	private Mock<IProductRepository> _productRepository = null!;
	private Mock<IStoreStockService> _storeStockService = null!;
	private Mock<IStoreService> _storeService = null!;
	private Mock<IUnitOfWork> _unitOfWork = null!;
	private ProductService _sut = null!;

	private Store _store = null!;

	[SetUp]
	public void SetUp(){
		_productRepository = new Mock<IProductRepository>();
		_storeStockService = new Mock<IStoreStockService>();
		_storeService = new Mock<IStoreService>();
		_unitOfWork = new Mock<IUnitOfWork>();

		_unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
			.Returns<Func<Task>>(action => action());

		_store = TestData.NewStore();
		_storeService.Setup(s => s.GetAllStoresAsync()).ReturnsAsync([_store]);

		StockFromRequest(TestData.NewStock(Barcode, _store.Id, 5));

		_sut = new ProductService(_productRepository.Object, _storeStockService.Object, _storeService.Object,
			_unitOfWork.Object);
	}

	private void StockFromRequest(params StoreStock[] stock){
		_storeStockService.Setup(s => s.CreateStoreStockFromJson(It.IsAny<object>(), It.IsAny<Product>()))
			.Returns(stock.ToList());
	}

	private static AcceptProductReq Req(Product product){
		return new AcceptProductReq{
			Product = JObject.FromObject(product),
			StoreQuantities = new JArray()
		};
	}

	[Test]
	public void AddProduct_WithoutPrice_Throws(){
		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddProductAsync(Req(TestData.NewProduct(price: null))));

		Assert.That(ex!.Message, Is.EqualTo("Price is required - set a price before saving the product"));
		_unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Never);
	}

	[TestCase(0)]
	[TestCase(-1)]
	public void AddProduct_NonPositivePrice_Throws(decimal price){
		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddProductAsync(Req(TestData.NewProduct(price: price))));

		Assert.That(ex!.Message, Is.EqualTo("Price must be greater than zero"));
	}

	[Test]
	public void AddProduct_PriceAboveCeiling_Throws(){
		var ex = Assert.ThrowsAsync<Exception>(() =>
			_sut.AddProductAsync(Req(TestData.NewProduct(price: 10_000_001m))));

		Assert.That(ex!.Message, Does.StartWith("Price cannot be higher than"));
	}

	[Test]
	public async Task AddProduct_PriceAtCeiling_IsAccepted(){
		await _sut.AddProductAsync(Req(TestData.NewProduct(price: 10_000_000m)));

		_productRepository.Verify(r => r.CreateProductAsync(It.IsAny<Product>()), Times.Once);
	}

	[TestCase(19.999)]
	[TestCase(0.001)]
	public void AddProduct_MoreThanTwoDecimals_Throws(decimal price){
		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddProductAsync(Req(TestData.NewProduct(price: price))));

		Assert.That(ex!.Message, Is.EqualTo("Price cannot have more than 2 decimal places"));
	}

	[TestCase(19.99)]
	[TestCase(20)]
	[TestCase(0.01)]
	public async Task AddProduct_TwoDecimalsOrFewer_IsAccepted(decimal price){
		await _sut.AddProductAsync(Req(TestData.NewProduct(price: price)));

		_productRepository.Verify(r => r.CreateProductAsync(It.IsAny<Product>()), Times.Once);
	}

	[Test]
	public void AddProduct_WithoutAnyStore_Throws(){
		StockFromRequest();

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddProductAsync(Req(TestData.NewProduct())));

		Assert.That(ex!.Message,
			Is.EqualTo("A product needs at least one store - create a store before adding products"));
		_unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Never);
	}

	[Test]
	public void AddProduct_ReferencingUnknownStore_Throws(){
		var unknownId = Guid.CreateVersion7();
		StockFromRequest(TestData.NewStock(Barcode, unknownId, 5));

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.AddProductAsync(Req(TestData.NewProduct())));

		Assert.That(ex!.Message, Is.EqualTo($"Store {unknownId} not found - refresh the page and try again"));
	}

	[Test]
	public async Task AddProduct_Valid_WritesProductAndStockInOneTransaction(){
		var stock = TestData.NewStock(Barcode, _store.Id, 5);
		StockFromRequest(stock);

		var product = await _sut.AddProductAsync(Req(TestData.NewProduct(price: 24.50m)));

		Assert.That(product.Price, Is.EqualTo(24.50m));
		_unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Once);
		_productRepository.Verify(r => r.CreateProductAsync(It.Is<Product>(p => p.Barcode == Barcode)), Times.Once);
		_storeStockService.Verify(s => s.CreateStoreStock(It.Is<List<StoreStock>>(l => l.Contains(stock))), Times.Once);
	}

	[Test]
	public void UpdateProduct_WithoutPrice_Throws(){
		var ex = Assert.ThrowsAsync<Exception>(() => _sut.UpdateProductAsync(Req(TestData.NewProduct(price: null))));

		Assert.That(ex!.Message, Is.EqualTo("Price is required - set a price before saving the product"));
		_productRepository.Verify(r => r.UpdateProduct(It.IsAny<Product>()), Times.Never);
	}

	[Test]
	public async Task UpdateProduct_Valid_UpdatesProductAndStock(){
		var (product, stock) = await _sut.UpdateProductAsync(Req(TestData.NewProduct()));

		Assert.That(product.Barcode, Is.EqualTo(Barcode));
		Assert.That(stock, Has.Count.EqualTo(1));
		_productRepository.Verify(r => r.UpdateProduct(It.Is<Product>(p => p.Barcode == Barcode)), Times.Once);
		_storeStockService.Verify(s => s.UpdateStock(It.IsAny<List<StoreStock>>(), Barcode), Times.Once);
	}

	[Test]
	public async Task RecalculateInStock_StockLeftAnywhere_MarksInStock(){
		_storeStockService.Setup(s => s.GetStoreStockFromId(Barcode)).ReturnsAsync([
			TestData.NewStock(Barcode, _store.Id, 0),
			TestData.NewStock(Barcode, Guid.CreateVersion7(), 3)
		]);

		await _sut.RecalculateInStockAsync(Barcode);

		_productRepository.Verify(r => r.UpdateInStockAsync(Barcode, true), Times.Once);
	}

	[Test]
	public async Task RecalculateInStock_NothingLeftAnywhere_MarksOutOfStock(){
		_storeStockService.Setup(s => s.GetStoreStockFromId(Barcode)).ReturnsAsync([
			TestData.NewStock(Barcode, _store.Id, 0)
		]);

		await _sut.RecalculateInStockAsync(Barcode);

		_productRepository.Verify(r => r.UpdateInStockAsync(Barcode, false), Times.Once);
	}

	[Test]
	public void GetFiltered_UnknownProductType_Throws(){
		var ex = Assert.ThrowsAsync<Exception>(() => _sut.GetFilteredAsync(1, 10, null, 99, null, null));

		Assert.That(ex!.Message, Is.EqualTo("Type not valid"));
	}

	[Test]
	public async Task GetFiltered_MapsArgumentsIntoFilter(){
		FilterReq? passed = null;
		_productRepository.Setup(r => r.GetFilteredAsync(It.IsAny<FilterReq>()))
			.Callback((FilterReq f) => passed = f)
			.ReturnsAsync((new List<Product>(), 0));

		await _sut.GetFilteredAsync(2, 20, "beatles", (int)ProductType.Vinyl, 10m, 50m);

		Assert.Multiple(() => {
			Assert.That(passed!.Page, Is.EqualTo(2));
			Assert.That(passed.PerPage, Is.EqualTo(20));
			Assert.That(passed.Search, Is.EqualTo("beatles"));
			Assert.That(passed.Type, Is.EqualTo(ProductType.Vinyl));
			Assert.That(passed.PriceLow, Is.EqualTo(10m));
			Assert.That(passed.PriceHigh, Is.EqualTo(50m));
		});
	}

	[Test]
	public void GetPage_WithoutPageSize_Throws(){
		var ex = Assert.ThrowsAsync<Exception>(() => _sut.GetPage(1, null));

		Assert.That(ex!.Message, Is.EqualTo("Need to return some items"));
	}

	[Test]
	public async Task GetPage_DefaultsToFirstPage(){
		await _sut.GetPage(null, 10);

		_productRepository.Verify(r => r.GetPage(1, 10), Times.Once);
	}

	[Test]
	public void FetchProducts_BarcodeAlreadyInDatabase_Throws(){
		_productRepository.Setup(r => r.ExistsProductId(Barcode)).ReturnsAsync(true);

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.FetchProducts(new AddProductReq{ Code = Barcode }));

		Assert.That(ex!.Message,
			Is.EqualTo("Product with this barcode is already in the database. Update it instead"));
	}
}
