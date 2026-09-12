using app.Models;
using app.Repositories;

namespace app.Services;

public interface ICartService{
	Task<CartView> AddItemAsync(Guid? cartId, string barcode, Guid storeId, int quantity);
	Task<CartView?> UpdateItemAsync(Guid cartId, string barcode, Guid storeId, int quantity);
	Task<CartView?> RemoveItemAsync(Guid cartId, string barcode, Guid storeId);
	Task<CartView?> GetCartAsync(Guid cartId, Guid? requesterId = null);
	Task<CartView?> GetCartForUserAsync(Guid userId);
	Task<Guid?> GetCartIdForUserAsync(Guid userId);
	Task<Guid> GetOrCreateCartIdForUserAsync(Guid userId);
	Task<CartMergeResult> MergeGuestCartAsync(Guid guestCartId, Guid userId);
	Task DeleteCartAsync(Guid cartId);
}

public class CartService: ICartService{
	private readonly ICartRepository _cartRepository;
	private readonly IProductService _productService;
	private readonly IStoreService _storeService;

	public CartService(ICartRepository cartRepository, IProductService productService, IStoreService storeService){
		_cartRepository = cartRepository;
		_productService = productService;
		_storeService = storeService;
	}

	public async Task<CartView> AddItemAsync(Guid? cartId, string barcode, Guid storeId, int quantity){
		if(quantity <= 0)
			throw new Exception("Quantity must be greater than zero");

		var product = await _productService.GetByIdAsync(barcode);

		if(!product.InStock)
			throw new Exception($"Product {barcode} is out of stock");

		if(product.Price == null)
			throw new Exception($"Product {barcode} is not available for purchase yet");

		var existingCart = cartId != null ?
						   await _cartRepository.GetCartAsync(cartId.Value) :
						   null;
		
		var existingItem = existingCart?.Items.FirstOrDefault(i => i.ProductBarcode == barcode && i.StoreId == storeId);

		await ValidateStoreStockAvailable(barcode, storeId, (existingItem?.Quantity ?? 0) + quantity);

		var cart = existingCart ?? await _cartRepository.CreateCartAsync();

		await _cartRepository.AddOrUpdateItemAsync(cart.Id, barcode, storeId, quantity);

		return await BuildCartView(cart.Id);
	}

	public async Task<CartView?> UpdateItemAsync(Guid cartId, string barcode, Guid storeId, int quantity){
		if(quantity > 0)
			await ValidateStoreStockAvailable(barcode, storeId, quantity);

		await _cartRepository.SetItemQuantityAsync(cartId, barcode, storeId, quantity);
		return await BuildCartViewOrDeleteIfEmpty(cartId);
	}

	public async Task<CartView?> RemoveItemAsync(Guid cartId, string barcode, Guid storeId){
		await _cartRepository.RemoveItemAsync(cartId, barcode, storeId);
		return await BuildCartViewOrDeleteIfEmpty(cartId);
	}

	public async Task<CartView?> GetCartAsync(Guid cartId, Guid? requesterId = null){
		var cart = await _cartRepository.GetCartAsync(cartId);

		if(cart == null)
			return null;

		if(cart.UserId != null && cart.UserId != requesterId)
			return null;

		return await BuildCartView(cartId);
	}

	public async Task<CartView?> GetCartForUserAsync(Guid userId){
		var cart = await _cartRepository.GetCartByUserAsync(userId);
		return cart == null ? null : await BuildCartView(cart.Id);
	}

	public async Task<Guid?> GetCartIdForUserAsync(Guid userId){
		var cart = await _cartRepository.GetCartByUserAsync(userId);
		return cart?.Id;
	}

	public async Task<Guid> GetOrCreateCartIdForUserAsync(Guid userId){
		var cart = await _cartRepository.GetCartByUserAsync(userId) ??
		           await _cartRepository.CreateCartAsync(userId);
		return cart.Id;
	}

	public async Task<CartMergeResult> MergeGuestCartAsync(Guid guestCartId, Guid userId){
		var guestCart = await _cartRepository.GetCartAsync(guestCartId);

		if(guestCart == null || guestCart.UserId != null || guestCart.Items.Count == 0)
			return new CartMergeResult{
				Cart = await GetCartForUserAsync(userId),
				MergedItems = 0,
				Capped = [],
				Dropped = []
			};

		var userCart = await _cartRepository.GetCartByUserAsync(userId);

		if(userCart == null){
			await _cartRepository.AttachCartToUserAsync(guestCart.Id, userId);
			return new CartMergeResult{
				Cart = await BuildCartView(guestCart.Id),
				MergedItems = guestCart.Items.Count,
				Capped = [],
				Dropped = []
			};
		}

		var merged = 0;
		var capped = new List<string>();
		var dropped = new List<string>();

		foreach(var item in guestCart.Items){
			var available = await AvailableAtStoreAsync(item.ProductBarcode, item.StoreId);

			if(available == 0){
				dropped.Add(await ProductNameAsync(item.ProductBarcode));
				continue;
			}

			var existing = userCart.Items
				.FirstOrDefault(i => i.ProductBarcode == item.ProductBarcode && i.StoreId == item.StoreId);

			var current = existing?.Quantity ?? 0;
			var wanted = current + item.Quantity;
			var total = Math.Min(wanted, available);

			if(total < wanted)
				capped.Add(await ProductNameAsync(item.ProductBarcode));

			if(total == current)
				continue;

			if(existing != null)
				await _cartRepository.SetItemQuantityAsync(userCart.Id, item.ProductBarcode, item.StoreId, total);
			else
				await _cartRepository.AddOrUpdateItemAsync(userCart.Id, item.ProductBarcode, item.StoreId, total);

			merged++;
		}

		await _cartRepository.DeleteCartAsync(guestCart.Id);

		return new CartMergeResult{
			Cart = await BuildCartView(userCart.Id),
			MergedItems = merged,
			Capped = capped,
			Dropped = dropped
		};
	}

	public async Task DeleteCartAsync(Guid cartId){
		await _cartRepository.DeleteCartAsync(cartId);
	}

	private async Task<int> AvailableAtStoreAsync(string barcode, Guid storeId){
		var stores = await _productService.GetAvailableStoresByIdAsync(barcode);
		return stores.FirstOrDefault(s => s.StoreId == storeId)?.Quantity ?? 0;
	}

	private async Task<string> ProductNameAsync(string barcode){
		var product = await _productService.GetByIdAsync(barcode);
		return product.Name;
	}
	
	private async Task<CartView?> BuildCartViewOrDeleteIfEmpty(Guid cartId){
		var cart = await _cartRepository.GetCartAsync(cartId) ??
		           throw new Exception("Cart not found");

		if(cart.Items.Count == 0){
			await _cartRepository.DeleteCartAsync(cartId);
			return null;
		}

		return await BuildCartView(cartId);
	}

	private async Task ValidateStoreStockAvailable(string barcode, Guid storeId, int totalQuantity){
		var stores = await _productService.GetAvailableStoresByIdAsync(barcode);
		var storeStock = stores.FirstOrDefault(s => s.StoreId == storeId) ??
		                  throw new Exception("Selected store does not carry this product");

		if(totalQuantity > storeStock.Quantity)
			throw new Exception($"Only {storeStock.Quantity} of {barcode} available at the selected store");
	}

	private async Task<CartView> BuildCartView(Guid cartId){
		var cart = await _cartRepository.GetCartAsync(cartId) ??
		           throw new Exception("Cart not found");

		var items = new List<CartItemView>();
		foreach(var item in cart.Items){
			var product = await _productService.GetByIdAsync(item.ProductBarcode);
			var store = await _storeService.GetStoreByIdAsync(item.StoreId);
			items.Add(new CartItemView{
				Barcode = product.Barcode,
				Name = product.Name,
				Artist = product.Artist,
				ImageUrl = product.ImageUrl,
				Price = product.Price,
				Quantity = item.Quantity,
				StoreId = item.StoreId,
				StoreName = store.Name
			});
		}

		return new CartView{ Id = cart.Id, Items = items };
	}
}
