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
		var store =  await _dbContext.Stores.FindAsync(id) ?? 
		             throw new Exception($"Store with id {id} doesnt exist");

		var storeStock = await _dbContext.StoreStocks.FirstOrDefaultAsync(s => s.StoreId == id) ??
		                 throw new Exception($"StoreStock object with store id {id} doesnt exist");
		
		_dbContext.StoreStocks.Remove(storeStock);
		_dbContext.Stores.Remove(store);
		await _dbContext.SaveChangesAsync();
	}
}