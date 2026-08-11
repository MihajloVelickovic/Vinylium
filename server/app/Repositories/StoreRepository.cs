using app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace app.Repositories;

public interface IStoreRepository{
	Task CreateStoreAsync(Store store);
	Task DeleteStoreAsync(int id);
	Task<List<Store>?> GetAllStoresAsync();
	Task<Store> GetStoreByIdAsync(int id);
	Task<Store> UpdateStoreAsync(int id, Store change);
	Task<List<Store>?> GetStoresAsync();
}

public class StoreRepository: IStoreRepository{

	private readonly VinyliumContext _dbContext;
	private readonly IDistributedCache _cache;

	public StoreRepository(VinyliumContext dbContext, IDistributedCache cache){
		_dbContext = dbContext;
		_cache = cache;
	}

	private async Task InvalidateStoresCacheAsync(bool invalidator){
		try{
			await _cache.RemoveAsync("allstores");
			if(!invalidator)
				await _cache.RemoveAsync("stores");
		}
		catch (Exception e){
			Console.Error.WriteLine($"Failed to invalidate allstores cache: {e.Message}");
		}
	}

	private async Task EnsureNoConflictAsync(string name, string conactNumber, int? excludeId = null)
	{
		var conflict = await _dbContext.Stores.AnyAsync(s =>
			(excludeId == null || s.Id != excludeId) &&
			(s.Name == name || s.ContactNumber == conactNumber));

		if (conflict)
			throw new Exception("Another store already uses that name or contact number");

	}

	public async Task CreateStoreAsync(Store store){
		await EnsureNoConflictAsync(store.Name, store.ContactNumber);
		var dbStore = await _dbContext.Stores.AddAsync(store) ??
		              throw new Exception("Failed to add store to database");
		var changes = await _dbContext.SaveChangesAsync();
		if(changes == 0)
			throw new Exception("Failed to write store to database");

		await InvalidateStoresCacheAsync(store.IsWarehouse);

	}

	public async Task DeleteStoreAsync(int id){
		var store =  await _dbContext.Stores.FindAsync(id) ?? 
		             throw new Exception($"Store with id {id} doesnt exist");

		var storeStock = await _dbContext.StoreStocks
			.Where(s => s.StoreId == id)
			.ToListAsync();
		
		if(storeStock.Count > 0)
			_dbContext.StoreStocks.RemoveRange(storeStock);
		
		_dbContext.Stores.Remove(store);
		await _dbContext.SaveChangesAsync();
		await InvalidateStoresCacheAsync(store.IsWarehouse);
	}

	public async Task<List<Store>?> GetAllStoresAsync(){
		
		List<Store>? stores;
		var st = await _cache.GetStringAsync("allstores");
		if(string.IsNullOrEmpty(st)){
			stores = await _dbContext.Stores.ToListAsync();
			await _cache.SetStringAsync("allstores", JsonConvert.SerializeObject(stores));
		}
		else
			stores = JsonConvert.DeserializeObject<List<Store>>(st);
		return stores;
	}

	public async Task<Store> GetStoreByIdAsync(int id){
		return await _dbContext.Stores.FirstOrDefaultAsync(s => s.Id == id) ??
		       throw new Exception($"Store with id {id} doesn't exist");
	}

	public async Task<Store> UpdateStoreAsync(int id, Store change){
		var store = await _dbContext.Stores.FirstOrDefaultAsync(s => s.Id == id) ??
		            throw new Exception($"Store with id {id} doesn't exist");
		await EnsureNoConflictAsync(change.Name, change.ContactNumber, id);

		store.Name = change.Name;
		store.Address = change.Address;
		store.City = change.City;
		store.ContactNumber = change.ContactNumber;
		store.OpeningTime = change.OpeningTime;
		store.ClosingTime = change.ClosingTime;
		store.IsWarehouse = change.IsWarehouse;

		await _dbContext.SaveChangesAsync();
		await InvalidateStoresCacheAsync(store.IsWarehouse);

		return store;
	}

	public async Task<List<Store>?> GetStoresAsync(){
		List<Store>? stores;
		var cached = await _cache.GetStringAsync("stores");
		if (string.IsNullOrEmpty(cached)){
			stores = await _dbContext.Stores.Where(s => !s.IsWarehouse)
										  .ToListAsync();
			await _cache.SetStringAsync("stores",JsonConvert.SerializeObject(stores));
		}
		else
			stores = JsonConvert.DeserializeObject<List<Store>>(cached);
		return stores;

	}
}