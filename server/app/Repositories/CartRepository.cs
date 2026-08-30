using app.Models;
using Microsoft.EntityFrameworkCore;

namespace app.Repositories;

public interface ICartRepository{
	Task<Cart> CreateCartAsync();
	Task<Cart?> GetCartAsync(Guid id);
	Task AddOrUpdateItemAsync(Guid cartId, string barcode, Guid storeId, int quantity);
	Task SetItemQuantityAsync(Guid cartId, string barcode, Guid storeId, int quantity);
	Task RemoveItemAsync(Guid cartId, string barcode, Guid storeId);
	Task DeleteCartAsync(Guid cartId);
}

public class CartRepository: ICartRepository{
	private readonly VinyliumContext _dbContext;

	public CartRepository(VinyliumContext dbContext){
		_dbContext = dbContext;
	}

	public async Task<Cart> CreateCartAsync(){
		var cart = new Cart();
		await _dbContext.Carts.AddAsync(cart);
		await _dbContext.SaveChangesAsync();
		return cart;
	}

	public async Task<Cart?> GetCartAsync(Guid id){
		return await _dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
	}

	public async Task AddOrUpdateItemAsync(Guid cartId, string barcode, Guid storeId, int quantity){
		var item = await _dbContext.CartItems.FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductBarcode == barcode && i.StoreId == storeId);

		if(item != null)
			item.Quantity += quantity;
		else
			await _dbContext.CartItems.AddAsync(new CartItem{
				CartId = cartId,
				ProductBarcode = barcode,
				StoreId = storeId,
				Quantity = quantity
			});

		await _dbContext.SaveChangesAsync();
	}

	public async Task SetItemQuantityAsync(Guid cartId, string barcode, Guid storeId, int quantity){
		var item = await _dbContext.CartItems.FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductBarcode == barcode && i.StoreId == storeId) ??
		           throw new Exception($"Item {barcode} not found in cart");

		if(quantity <= 0)
			_dbContext.CartItems.Remove(item);
		else
			item.Quantity = quantity;

		await _dbContext.SaveChangesAsync();
	}

	public async Task RemoveItemAsync(Guid cartId, string barcode, Guid storeId){
		var item = await _dbContext.CartItems
			.FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductBarcode == barcode && i.StoreId == storeId);

		if(item == null)
			return;

		_dbContext.CartItems.Remove(item);
		await _dbContext.SaveChangesAsync();
	}

	public async Task DeleteCartAsync(Guid cartId){
		var cart = await _dbContext.Carts.FirstOrDefaultAsync(c => c.Id == cartId);
		if(cart == null)
			return;

		_dbContext.Carts.Remove(cart);
		await _dbContext.SaveChangesAsync();
	}
}
