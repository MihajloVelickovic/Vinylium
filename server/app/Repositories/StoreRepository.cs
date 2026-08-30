using System.Reflection.Metadata;
using app.Models;
using app.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace app.Repositories;

public interface IStoreRepository{
	Task<Store> CreateStoreAsync(Store store);
	Task DeleteStoreAsync(int id);
	Task<List<Store>?> GetAllStoresAsync();
	Task<Store> GetStoreByIdAsync(int id);
	Task<Store> UpdateStoreAsync(int id, Store change);
	Task<List<Store>?> GetStoresAsync();
	public Task<bool> HasWarehouse();
	Task<int?> GetWarehouseId();
	Task<(List<Store> result, int pages)> GetFilteredAsync(StoreFilterReq req);
}

public class StoreRepository: IStoreRepository{

	private readonly VinyliumContext _dbContext;
	private readonly IDistributedCache _cache;

	public StoreRepository(VinyliumContext dbContext, IDistributedCache cache){
		_dbContext = dbContext;
		_cache = cache;
	}
	public async Task<bool> HasWarehouse(){
		var cached = await _cache.GetStringAsync("warehouse");
		if(string.IsNullOrEmpty(cached)){
			var exists = await _dbContext.Stores.Where(s => s.IsWarehouse).SingleOrDefaultAsync() != null;
			await _cache.SetStringAsync("warehouse", JsonConvert.SerializeObject(exists));
			return exists;
		}
		return JsonConvert.DeserializeObject<bool>(cached);
	}

	public async Task<int?> GetWarehouseId(){
		try{
			var x = await _dbContext.Stores.Where(s => s.IsWarehouse).SingleOrDefaultAsync();
			return x?.Id;
		}
		catch(Exception){
			return null;
		}
	}

	public async Task<Store> CreateStoreAsync(Store store){
		if(store.IsWarehouse && await HasWarehouse())
			throw new Exception("Warehouse already created");
		var dbStore = await _dbContext.Stores.AddAsync(store) ??
		              throw new Exception("Failed to add store to database");
		var changes = await _dbContext.SaveChangesAsync();
		if(changes == 0)
			throw new Exception("Failed to write store to database");
		await _cache.SetStringAsync("stores", JsonConvert.SerializeObject(await _dbContext.Stores.ToListAsync()));
		if(store.IsWarehouse)
			await _cache.SetStringAsync("warehouse", JsonConvert.SerializeObject(true));
		return dbStore.Entity;
	}

	public async Task DeleteStoreAsync(int id){
		var store =  await _dbContext.Stores.FindAsync(id) ?? 
		             throw new Exception($"Store with id {id} doesnt exist");
		
		_dbContext.Stores.Remove(store);
		var changes = await _dbContext.SaveChangesAsync();
		if(changes > 0){
			await _cache.SetStringAsync("stores", JsonConvert.SerializeObject(await _dbContext.Stores.ToListAsync()));
			if(store.IsWarehouse)
				await _cache.SetStringAsync("warehouse", JsonConvert.SerializeObject(false));
		}
	}

	public async Task<List<Store>?> GetAllStoresAsync(){
		List<Store>? stores;
		var st = await _cache.GetStringAsync("stores");
		if(string.IsNullOrEmpty(st)){
			stores = await _dbContext.Stores.OrderBy(s => s.Id).ToListAsync();
			await _cache.SetStringAsync("stores", JsonConvert.SerializeObject(stores));
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
		
		store.Name = change.Name;
		store.Address = change.Address;
		store.City = change.City;
		store.ContactNumber = change.ContactNumber;
		store.OpeningTime = change.OpeningTime;
		store.ClosingTime = change.ClosingTime;
		store.IsWarehouse = change.IsWarehouse;
		
		var changes = await _dbContext.SaveChangesAsync();
		if(changes > 0){
			await _cache.SetStringAsync("stores", JsonConvert.SerializeObject(await _dbContext.Stores.ToListAsync()));
			await _cache.SetStringAsync("warehouse", JsonConvert.SerializeObject(change.IsWarehouse));
		}
		return store;
	}

	public async Task<List<Store>?> GetStoresAsync(){
		var stores = await GetAllStoresAsync();
		var wh = stores?.Find(s => s.IsWarehouse);
		if(wh != null)
			stores?.Remove(wh);
		return stores;
	}

	public async Task<(List<Store> result, int pages)> GetFilteredAsync(StoreFilterReq req){

		var query = _dbContext.Stores.AsQueryable();

		var page = req.Page ?? 1;
		var perPage = req.PerPage ?? 20;
		var skip = (page - 1) * perPage;

		if(!string.IsNullOrWhiteSpace(req.Search))
			query = query.Where(s => EF.Functions.ILike(s.Name, $"%{req.Search}%") ||
			                         EF.Functions.ILike(s.Address, $"%{req.Search}%") ||
			                         EF.Functions.ILike(s.City, $"%{req.Search}%") ||
			                         EF.Functions.ILike(s.ContactNumber, $"%{req.Search}%"));

		if(req.IsWarehouse != null)
			query = query.Where(s => s.IsWarehouse == req.IsWarehouse);

		var totalCount = await query.CountAsync();
		var pages = totalCount / perPage + 1;
		var result = await query.OrderBy(s => s.Id).Skip(skip).Take(perPage).ToListAsync();
		return (result, pages);

	}
}