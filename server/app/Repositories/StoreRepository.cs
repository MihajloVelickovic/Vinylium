using app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace app.Repositories;

public interface IStoreRepository{
	Task CreateStoreAsync(Store store);
	Task DeleteStoreAsync(int id);
	Task<List<Store>?> GetAllStoresAsync();
}

public class StoreRepository: IStoreRepository{

	private readonly VinyliumContext _dbContext;
	private readonly IDistributedCache _cache;

	public StoreRepository(VinyliumContext dbContext, IDistributedCache cache){
		_dbContext = dbContext;
		_cache = cache;
	}

	public async Task CreateStoreAsync(Store store){
		var dbStore = await _dbContext.Stores.AddAsync(store) ??
		              throw new Exception("Failed to add store to database");
		var changes = await _dbContext.SaveChangesAsync();
		if(changes == 0)
			throw new Exception("Failed to write store to database");
		
		await _cache.RemoveAsync("allstores");
		
	}

	public async Task DeleteStoreAsync(int id){
		var store =  await _dbContext.Stores.FindAsync(id) ?? 
		             throw new Exception($"Store with id {id} doesnt exist");

		var storeStock = await _dbContext.StoreStocks.FirstOrDefaultAsync(s => s.StoreId == id) ??
		                 throw new Exception($"StoreStock object with store id {id} doesnt exist");
		
		_dbContext.StoreStocks.Remove(storeStock);
		_dbContext.Stores.Remove(store);
		await _dbContext.SaveChangesAsync();
		await _cache.RemoveAsync("allstores");
		
	}

	public async Task<List<Store>?> GetAllStoresAsync(){
		
		List<Store>? stores;
		var st = await _cache.GetStringAsync("allstores");
		if(string.IsNullOrEmpty(st)){
			stores = await _dbContext.Stores.ToListAsync();
			await _cache.SetStringAsync("allstores", JsonConvert.SerializeObject(stores));
		}
		else{
			stores = JsonConvert.DeserializeObject<List<Store>>(st);
		}
		return stores;
	}
}