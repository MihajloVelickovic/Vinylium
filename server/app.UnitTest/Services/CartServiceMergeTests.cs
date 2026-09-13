using app.Models;
using app.Repositories;
using app.Services;
using Moq;

namespace app.UnitTest.Services;

[TestFixture]
public class CartServiceMergeTests{
	private const string Barcode = "5099969944123";

	private Mock<ICartRepository> _cartRepository = null!;
	private Mock<IProductService> _productService = null!;
	private Mock<IStoreService> _storeService = null!;
	private CartService _sut = null!;

	private Guid _userId;
	private Guid _storeId;
	private Guid _guestCartId;
	private Guid _userCartId;

	[SetUp]
	public void SetUp(){
		_cartRepository = new Mock<ICartRepository>();
		_productService = new Mock<IProductService>();
		_storeService = new Mock<IStoreService>();
		_sut = new CartService(_cartRepository.Object, _productService.Object, _storeService.Object);

		_userId = Guid.CreateVersion7();
		_storeId = Guid.CreateVersion7();
		_guestCartId = Guid.CreateVersion7();
		_userCartId = Guid.CreateVersion7();

		_productService.Setup(p => p.GetByIdAsync(Barcode))
			.ReturnsAsync(TestData.NewProduct(Barcode));
		_storeService.Setup(s => s.GetStoreByIdAsync(_storeId))
			.ReturnsAsync(TestData.NewStore(_storeId));
	}

	private void StockIs(int quantity){
		_productService.Setup(p => p.GetAvailableStoresByIdAsync(Barcode))
			.ReturnsAsync([TestData.NewStock(Barcode, _storeId, quantity)]);
	}

	private void GuestCartIs(Cart? cart){
		_cartRepository.Setup(c => c.GetCartAsync(_guestCartId)).ReturnsAsync(cart);
	}

	private void UserCartIs(Cart? cart){
		_cartRepository.Setup(c => c.GetCartByUserAsync(_userId)).ReturnsAsync(cart);
		if(cart != null)
			_cartRepository.Setup(c => c.GetCartAsync(cart.Id)).ReturnsAsync(cart);
	}

	private Cart GuestCartWith(int quantity){
		return TestData.NewCart(_guestCartId, TestData.NewCartItem(Barcode, _storeId, quantity, _guestCartId));
	}

	private Cart UserCartWith(int quantity){
		return TestData.NewOwnedCart(_userCartId, _userId,
			TestData.NewCartItem(Barcode, _storeId, quantity, _userCartId));
	}

	[Test]
	public async Task Merge_GuestCartGone_LeavesAccountCartAlone(){
		GuestCartIs(null);
		UserCartIs(UserCartWith(1));

		var result = await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		Assert.Multiple(() => {
			Assert.That(result.MergedItems, Is.EqualTo(0));
			Assert.That(result.Cart!.Id, Is.EqualTo(_userCartId));
			Assert.That(result.Cart.Items, Has.Count.EqualTo(1));
		});
		_cartRepository.Verify(c => c.DeleteCartAsync(It.IsAny<Guid>()), Times.Never);
	}

	[Test]
	public async Task Merge_EmptyGuestCart_MergesNothing(){
		GuestCartIs(TestData.NewCart(_guestCartId));
		UserCartIs(UserCartWith(1));

		var result = await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		Assert.That(result.MergedItems, Is.EqualTo(0));
		_cartRepository.Verify(c => c.AddOrUpdateItemAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
			It.IsAny<int>()), Times.Never);
	}

	[Test]
	public async Task Merge_GuestCartAlreadyClaimed_MergesNothing(){
		var otherUsersCart = TestData.NewOwnedCart(_guestCartId, Guid.CreateVersion7(),
			TestData.NewCartItem(Barcode, _storeId, 2, _guestCartId));
		GuestCartIs(otherUsersCart);
		UserCartIs(UserCartWith(1));

		var result = await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		Assert.That(result.MergedItems, Is.EqualTo(0));
		_cartRepository.Verify(c => c.DeleteCartAsync(It.IsAny<Guid>()), Times.Never);
		_cartRepository.Verify(c => c.AttachCartToUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
	}

	[Test]
	public async Task Merge_AccountHasNoCart_ClaimsGuestCartAsIs(){
		GuestCartIs(GuestCartWith(2));
		UserCartIs(null);

		var result = await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		_cartRepository.Verify(c => c.AttachCartToUserAsync(_guestCartId, _userId), Times.Once);
		_cartRepository.Verify(c => c.DeleteCartAsync(It.IsAny<Guid>()), Times.Never);
		Assert.Multiple(() => {
			Assert.That(result.MergedItems, Is.EqualTo(1));
			Assert.That(result.Cart!.Id, Is.EqualTo(_guestCartId));
			Assert.That(result.Capped, Is.Empty);
			Assert.That(result.Dropped, Is.Empty);
		});
	}

	[Test]
	public async Task Merge_SameProductInBothCarts_SumsQuantities(){
		StockIs(10);
		GuestCartIs(GuestCartWith(2));
		UserCartIs(UserCartWith(1));

		var result = await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		_cartRepository.Verify(c => c.SetItemQuantityAsync(_userCartId, Barcode, _storeId, 3), Times.Once);
		Assert.Multiple(() => {
			Assert.That(result.MergedItems, Is.EqualTo(1));
			Assert.That(result.Capped, Is.Empty);
		});
	}

	[Test]
	public async Task Merge_ProductOnlyInGuestCart_AddsItToAccountCart(){
		StockIs(10);
		GuestCartIs(GuestCartWith(2));
		UserCartIs(TestData.NewOwnedCart(_userCartId, _userId));

		await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		_cartRepository.Verify(c => c.AddOrUpdateItemAsync(_userCartId, Barcode, _storeId, 2), Times.Once);
	}

	[Test]
	public async Task Merge_CombinedQuantityAboveStock_CapsAtAvailableAndReportsIt(){
		StockIs(4);
		GuestCartIs(GuestCartWith(3));
		UserCartIs(UserCartWith(2));

		var result = await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		_cartRepository.Verify(c => c.SetItemQuantityAsync(_userCartId, Barcode, _storeId, 4), Times.Once);
		Assert.Multiple(() => {
			Assert.That(result.MergedItems, Is.EqualTo(1));
			Assert.That(result.Capped, Is.EqualTo(new[]{ "Abbey Road" }));
			Assert.That(result.Dropped, Is.Empty);
		});
	}

	[Test]
	public async Task Merge_AccountCartAlreadyAtStockLimit_ReportsCapWithoutWriting(){
		StockIs(3);
		GuestCartIs(GuestCartWith(2));
		UserCartIs(UserCartWith(3));

		var result = await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		_cartRepository.Verify(c => c.SetItemQuantityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
			It.IsAny<int>()), Times.Never);
		Assert.Multiple(() => {
			Assert.That(result.MergedItems, Is.EqualTo(0));
			Assert.That(result.Capped, Is.EqualTo(new[]{ "Abbey Road" }));
		});
	}

	[Test]
	public async Task Merge_StoreNoLongerCarriesProduct_DropsItAndReportsIt(){
		StockIs(0);
		GuestCartIs(GuestCartWith(2));
		UserCartIs(TestData.NewOwnedCart(_userCartId, _userId));

		var result = await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		_cartRepository.Verify(c => c.AddOrUpdateItemAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(),
			It.IsAny<int>()), Times.Never);
		Assert.Multiple(() => {
			Assert.That(result.MergedItems, Is.EqualTo(0));
			Assert.That(result.Dropped, Is.EqualTo(new[]{ "Abbey Road" }));
			Assert.That(result.Capped, Is.Empty);
		});
	}

	[Test]
	public async Task Merge_DeletesGuestCartOnceMerged(){
		StockIs(10);
		GuestCartIs(GuestCartWith(1));
		UserCartIs(UserCartWith(1));

		await _sut.MergeGuestCartAsync(_guestCartId, _userId);

		_cartRepository.Verify(c => c.DeleteCartAsync(_guestCartId), Times.Once);
	}

	[Test]
	public async Task GetOrCreateCartIdForUser_NoCartYet_CreatesOneOwnedByTheUser(){
		UserCartIs(null);
		_cartRepository.Setup(c => c.CreateCartAsync(_userId))
			.ReturnsAsync(TestData.NewOwnedCart(_userCartId, _userId));

		var cartId = await _sut.GetOrCreateCartIdForUserAsync(_userId);

		Assert.That(cartId, Is.EqualTo(_userCartId));
		_cartRepository.Verify(c => c.CreateCartAsync(_userId), Times.Once);
	}

	[Test]
	public async Task GetOrCreateCartIdForUser_ExistingCart_ReusesIt(){
		UserCartIs(UserCartWith(1));

		var cartId = await _sut.GetOrCreateCartIdForUserAsync(_userId);

		Assert.That(cartId, Is.EqualTo(_userCartId));
		_cartRepository.Verify(c => c.CreateCartAsync(It.IsAny<Guid?>()), Times.Never);
	}

	[Test]
	public async Task GetCartIdForUser_NoCart_ReturnsNullWithoutCreating(){
		UserCartIs(null);

		Assert.That(await _sut.GetCartIdForUserAsync(_userId), Is.Null);
		_cartRepository.Verify(c => c.CreateCartAsync(It.IsAny<Guid?>()), Times.Never);
	}

	[Test]
	public async Task GetCartForUser_NoCart_ReturnsNull(){
		UserCartIs(null);

		Assert.That(await _sut.GetCartForUserAsync(_userId), Is.Null);
	}

	[Test]
	public async Task GetCart_OwnedByAnotherUser_ReturnsNull(){
		var stranger = Guid.CreateVersion7();
		_cartRepository.Setup(c => c.GetCartAsync(_userCartId))
			.ReturnsAsync(TestData.NewOwnedCart(_userCartId, stranger,
				TestData.NewCartItem(Barcode, _storeId, 1, _userCartId)));

		Assert.That(await _sut.GetCartAsync(_userCartId, _userId), Is.Null);
	}

	[Test]
	public async Task GetCart_OwnedByRequester_ReturnsView(){
		UserCartIs(UserCartWith(1));

		var view = await _sut.GetCartAsync(_userCartId, _userId);

		Assert.That(view, Is.Not.Null);
		Assert.That(view!.Id, Is.EqualTo(_userCartId));
	}

	[Test]
	public async Task GetCart_OwnedCartRequestedByGuest_ReturnsNull(){
		UserCartIs(UserCartWith(1));

		Assert.That(await _sut.GetCartAsync(_userCartId), Is.Null);
	}
}
