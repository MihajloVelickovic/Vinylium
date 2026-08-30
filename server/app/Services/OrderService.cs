using app.Helper;
using app.Models;
using app.Repositories;
using app.Requests;

namespace app.Services;

public interface IOrderService{
	Task<Order> CheckoutAsync(CheckoutReq req, Guid? userId);
	Task<List<Order>> GetMyOrdersAsync(Guid userId, string email);
	Task BackfillGuestOrdersAsync(Guid userId, string email);
	Task CancelOrderAsync(Guid orderId, Guid userId, string email);
}

public class OrderService: IOrderService{
	private static readonly TimeSpan CancellationWindow = TimeSpan.FromHours(int.Parse(DotEnv.Get("CANCELLATION_TIME") ?? "24"));

	private readonly IOrderRepository _orderRepository;
	private readonly ICartService _cartService;
	private readonly IStoreStockService _storeStockService;
	private readonly IProductService _productService;
	private readonly IUnitOfWork _unitOfWork;

	public OrderService(IOrderRepository orderRepository, ICartService cartService,
		IStoreStockService storeStockService, IProductService productService, IUnitOfWork unitOfWork){
		_orderRepository = orderRepository;
		_cartService = cartService;
		_storeStockService = storeStockService;
		_productService = productService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Order> CheckoutAsync(CheckoutReq req, Guid? userId){
		var cart = await _cartService.GetCartAsync(req.CartId) ??
		           throw new Exception("Cart not found");

		if(cart.Items.Count == 0)
			throw new Exception("Cart is empty");

		var orderItems = new List<OrderItem>();
		foreach(var item in cart.Items){
	
			var price = item.Price ?? throw new Exception($"Product {item.Barcode} has no price set");
			orderItems.Add(new OrderItem{
				ProductBarcode = item.Barcode,
				StoreId = item.StoreId,
				Quantity = item.Quantity,
				UnitPrice = price
			});
		}

		var order = new Order{
			Email = req.Email,
			UserId = userId,
			Items = orderItems
		};

		await _unitOfWork.ExecuteInTransactionAsync(async () => {
			foreach(var item in orderItems){
				await _storeStockService.DecrementStockAsync(item.StoreId, item.ProductBarcode, item.Quantity);
				await _productService.RecalculateInStockAsync(item.ProductBarcode);
			}
			await _orderRepository.CreateOrderAsync(order);
			await _cartService.DeleteCartAsync(cart.Id);
		});

		return order;
	}

	public async Task<List<Order>> GetMyOrdersAsync(Guid userId, string email){
		return await _orderRepository.GetOrdersForUserAsync(userId, email);
	}

	public async Task BackfillGuestOrdersAsync(Guid userId, string email){
		await _orderRepository.BackfillUserIdByEmailAsync(userId, email);
	}

	public async Task CancelOrderAsync(Guid orderId, Guid userId, string email){
		var order = await _orderRepository.GetByIdAsync(orderId) ??
		            throw new Exception("Order not found");

		if(!(order.UserId == userId || order.Email == email))
			throw new Exception("Order not found");

		if(DateTime.UtcNow - order.CreatedAt > CancellationWindow)
			throw new Exception("The 24 hour window for cancellation has passed");

		await _unitOfWork.ExecuteInTransactionAsync(async () => {
			foreach(var item in order.Items){
				await _storeStockService.IncrementStockAsync(item.StoreId, item.ProductBarcode, item.Quantity);
				await _productService.RecalculateInStockAsync(item.ProductBarcode);
			}
			await _orderRepository.DeleteOrderAsync(orderId);
		});
	}
}
