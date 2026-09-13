using app.Models;
using app.Repositories;
using app.Requests;
using app.Services;
using Moq;

namespace app.UnitTest.Services;

[TestFixture]
public class OrderServiceTests{
	private const string Barcode = "5099969944123";
	private const string Email = "strale@vinylium.rs";

	private Mock<IOrderRepository> _orderRepository = null!;
	private Mock<ICartService> _cartService = null!;
	private Mock<IStoreStockService> _storeStockService = null!;
	private Mock<IProductService> _productService = null!;
	private Mock<IUnitOfWork> _unitOfWork = null!;
	private OrderService _sut = null!;

	private Guid _cartId;
	private Guid _storeId;

	[SetUp]
	public void SetUp(){
		_orderRepository = new Mock<IOrderRepository>();
		_cartService = new Mock<ICartService>();
		_storeStockService = new Mock<IStoreStockService>();
		_productService = new Mock<IProductService>();
		_unitOfWork = new Mock<IUnitOfWork>();

		_unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
			.Returns<Func<Task>>(action => action());

		_cartId = Guid.CreateVersion7();
		_storeId = Guid.CreateVersion7();

		_sut = new OrderService(_orderRepository.Object, _cartService.Object, _storeStockService.Object,
			_productService.Object, _unitOfWork.Object);
	}

	private void CartIs(CartView? cart){
		_cartService.Setup(c => c.GetCartAsync(_cartId, It.IsAny<Guid?>())).ReturnsAsync(cart);
	}

	private CheckoutReq Req(){
		return new CheckoutReq{ CartId = _cartId, Email = Email };
	}

	[Test]
	public void Checkout_CartNotFound_Throws(){
		CartIs(null);

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.CheckoutAsync(Req(), null));

		Assert.That(ex!.Message, Is.EqualTo("Cart not found"));
		_unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Never);
	}

	[Test]
	public void Checkout_EmptyCart_Throws(){
		CartIs(TestData.NewCartView(_cartId));

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.CheckoutAsync(Req(), null));

		Assert.That(ex!.Message, Is.EqualTo("Cart is empty"));
	}

	[Test]
	public void Checkout_ItemWithoutPrice_ThrowsBeforeTouchingStock(){
		CartIs(TestData.NewCartView(_cartId, TestData.NewCartItemView(Barcode, _storeId, 1, price: null)));

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.CheckoutAsync(Req(), null));

		Assert.That(ex!.Message, Is.EqualTo($"Product {Barcode} has no price set"));
		_storeStockService.Verify(s => s.DecrementStockAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()),
			Times.Never);
		_orderRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
	}

	[Test]
	public async Task Checkout_Valid_DecrementsStockCreatesOrderAndClearsCart(){
		CartIs(TestData.NewCartView(_cartId, TestData.NewCartItemView(Barcode, _storeId, 2, price: 24.50m)));

		var order = await _sut.CheckoutAsync(Req(), null);

		Assert.Multiple(() => {
			Assert.That(order.Email, Is.EqualTo(Email));
			Assert.That(order.UserId, Is.Null);
			Assert.That(order.Items, Has.Count.EqualTo(1));
			Assert.That(order.Items.First().UnitPrice, Is.EqualTo(24.50m));
			Assert.That(order.Items.First().Quantity, Is.EqualTo(2));
		});
		_storeStockService.Verify(s => s.DecrementStockAsync(_storeId, Barcode, 2), Times.Once);
		_productService.Verify(p => p.RecalculateInStockAsync(Barcode), Times.Once);
		_orderRepository.Verify(r => r.CreateOrderAsync(order), Times.Once);
		_cartService.Verify(c => c.DeleteCartAsync(_cartId), Times.Once);
		_unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Once);
	}

	[Test]
	public async Task Checkout_SignedInUser_StampsOrderWithUserId(){
		var userId = Guid.CreateVersion7();
		CartIs(TestData.NewCartView(_cartId, TestData.NewCartItemView(Barcode, _storeId, 1)));

		var order = await _sut.CheckoutAsync(Req(), userId);

		Assert.That(order.UserId, Is.EqualTo(userId));
	}

	[Test]
	public void Cancel_UnknownOrder_Throws(){
		var orderId = Guid.CreateVersion7();
		_orderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.CancelOrderAsync(orderId, Guid.CreateVersion7(), Email));

		Assert.That(ex!.Message, Is.EqualTo("Order not found"));
	}

	[Test]
	public void Cancel_OrderOfAnotherUser_Throws(){
		var orderId = Guid.CreateVersion7();
		_orderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(new Order{
			Id = orderId, UserId = Guid.CreateVersion7(), Email = "someone@else.rs"
		});

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.CancelOrderAsync(orderId, Guid.CreateVersion7(), Email));

		Assert.That(ex!.Message, Is.EqualTo("Order not found"));
		_orderRepository.Verify(r => r.DeleteOrderAsync(It.IsAny<Guid>()), Times.Never);
	}

	[Test]
	public void Cancel_GuestOrderWithDifferentEmail_Throws(){
		var orderId = Guid.CreateVersion7();
		_orderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(new Order{
			Id = orderId, UserId = null, Email = "someone@else.rs"
		});

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.CancelOrderAsync(orderId, Guid.CreateVersion7(), Email));

		Assert.That(ex!.Message, Is.EqualTo("Order not found"));
	}

	[Test]
	public async Task Cancel_GuestOrderWithMatchingEmail_ReturnsStock(){
		var orderId = Guid.CreateVersion7();
		_orderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(new Order{
			Id = orderId,
			UserId = null,
			Email = Email,
			Items = [TestData.NewOrderItem(Barcode, _storeId, 2)]
		});

		await _sut.CancelOrderAsync(orderId, Guid.CreateVersion7(), Email);

		_storeStockService.Verify(s => s.IncrementStockAsync(_storeId, Barcode, 2), Times.Once);
		_productService.Verify(p => p.RecalculateInStockAsync(Barcode), Times.Once);
		_orderRepository.Verify(r => r.DeleteOrderAsync(orderId), Times.Once);
	}

	[Test]
	public async Task Cancel_OwnOrderInsideWindow_ReturnsStock(){
		var orderId = Guid.CreateVersion7();
		var userId = Guid.CreateVersion7();
		_orderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(new Order{
			Id = orderId,
			UserId = userId,
			Email = Email,
			CreatedAt = DateTime.UtcNow.AddHours(-23),
			Items = [TestData.NewOrderItem(Barcode, _storeId, 1)]
		});

		await _sut.CancelOrderAsync(orderId, userId, Email);

		_storeStockService.Verify(s => s.IncrementStockAsync(_storeId, Barcode, 1), Times.Once);
		_orderRepository.Verify(r => r.DeleteOrderAsync(orderId), Times.Once);
	}

	[Test]
	public void Cancel_AfterWindowClosed_ThrowsAndKeepsOrder(){
		var orderId = Guid.CreateVersion7();
		var userId = Guid.CreateVersion7();
		_orderRepository.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(new Order{
			Id = orderId,
			UserId = userId,
			Email = Email,
			CreatedAt = DateTime.UtcNow.AddHours(-25),
			Items = [TestData.NewOrderItem(Barcode, _storeId, 1)]
		});

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.CancelOrderAsync(orderId, userId, Email));

		Assert.That(ex!.Message, Is.EqualTo("The 24 hour window for cancellation has passed"));
		_orderRepository.Verify(r => r.DeleteOrderAsync(It.IsAny<Guid>()), Times.Never);
		_storeStockService.Verify(s => s.IncrementStockAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()),
			Times.Never);
	}
}
