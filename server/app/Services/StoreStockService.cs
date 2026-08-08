using app.Models;
using app.Repositories;
using Newtonsoft.Json.Linq;

namespace app.Services;


public interface IStoreStockService{
	List<StoreStock> CreateStoreStockFromJson(object reqStoreQuantities, string id);
	Task CreateStoreStock(List<StoreStock> stock);
	Task<List<StoreStock>> GetStoreStockFromId(string barcode);
}

public class StoreStockService:  IStoreStockService{
	private readonly IStoreStockRepository _repo;

	public StoreStockService(IStoreStockRepository repo){
		_repo = repo;
	}

	public List<StoreStock> CreateStoreStockFromJson(object reqStoreQuantities, string id){
		var storeStockJArrayString = reqStoreQuantities.ToString() ??
		                             throw new Exception("Failed to create product string from request data.");
		
		var storeStockJArray = JArray.Parse(storeStockJArrayString);
		
		var storeStock = storeStockJArray.ToObject<List<StoreStock>>() ?? 
		                 throw new Exception("Failed to cast json to product.");
		
		List<StoreStock> quantities = [];
		foreach(var s in storeStock){
			var x = new StoreStock{
				ProductBarcode =  id,
				StoreId = s.Store.Id,
				Quantity =  s.Quantity
			};
			quantities.Add(x);
		}

		return quantities;
	}
	
	public async Task CreateStoreStock(List<StoreStock> stock){
		await _repo.CreateStoreStockAsync(stock);
	}

	public async Task<List<StoreStock>> GetStoreStockFromId(string barcode){
		return await _repo.GetStoreStockFromId(barcode);
	}
}