using app.Models;
using Microsoft.EntityFrameworkCore;

namespace app.Repositories;

public interface IStoreRepository{
	Task CreateStoreAsync(Store store);
	Task DeleteStoreAsync(int id);
}

public class StoreRepository: IStoreRepository{

	private readonly VinyliumContext _dbContext;

	public StoreRepository(VinyliumContext dbContext){
		_dbContext = dbContext;
	}

	public async Task CreateStoreAsync(Store store){
		var dbStore = await _dbContext.Stores.AddAsync(store) ??
		                throw new Exception("Failed to add store to database");
		var changes = await _dbContext.SaveChangesAsync();
		if(changes == 0)
			throw new Exception("Failed to write store to database");
	}

	public async Task DeleteStoreAsync(int id){
		var store =  await _dbContext.Stores.Include(s => s.Products).FirstOrDefaultAsync(s => s.Id == id) ??
		             throw new Exception("Failed to get store from database");

		foreach(var product in store.Products){
			product.AvailableAt.Remove(product.AvailableAt.FirstOrDefault(s => s.Item1.Id == id) ?? 
			                           throw new Exception("Failed to remove product from database"));
			_dbContext.Products.Update(product);
		}
		
		_dbContext.Stores.Remove(store);
		await _dbContext.SaveChangesAsync();
	}
}