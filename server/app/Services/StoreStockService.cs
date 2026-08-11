using app.Models;
using app.Repositories;
using Newtonsoft.Json.Linq;

namespace app.Services;


public interface IStoreStockService{
	List<StoreStock> CreateStoreStockFromJson(object reqStoreQuantities, Product product);
	Task CreateStoreStock(List<StoreStock> stock);
	Task<List<StoreStock>> GetStoreStockFromId(string barcode);
	Task UpdateStock(List<StoreStock> storeStock, string barcode);
}

public class StoreStockService:  IStoreStockService{
	private readonly IStoreStockRepository _repo;

	public StoreStockService(IStoreStockRepository repo){
		_repo = repo;
	}

	public List<StoreStock> CreateStoreStockFromJson(object reqStoreQuantities, Product product){
		var storeStockJArrayString = reqStoreQuantities.ToString() ??
		                             throw new Exception("Failed to create product string from request data");
		
		var storeStockJArray = JArray.Parse(storeStockJArrayString);
		
		var storeStock = storeStockJArray.ToObject<List<StoreStock>>() ?? 
		                 throw new Exception("Failed to cast json to product");
		
		List<StoreStock> quantities = [];
		
		product.InStock = true;
		var totalQuantity = 0;
		foreach(var s in storeStock){
			if(s.Quantity < 0)
				throw new Exception("Quantity cant be negative");
			if(s.Quantity >= 0){
				var x = new StoreStock{
					ProductBarcode = product.Barcode,
					StoreId = s.Store.Id,
					Quantity = s.Quantity
				};
				quantities.Add(x);
			}
			totalQuantity +=  s.Quantity;
		}
		if(totalQuantity <= 0){
			product.InStock = false;
		}
		return quantities;
	}
	
	public async Task CreateStoreStock(List<StoreStock> stock){
		await _repo.CreateStoreStockAsync(stock);
	}

	public async Task<List<StoreStock>> GetStoreStockFromId(string barcode){
		return await _repo.GetStoreStockFromId(barcode);
	}

	public async Task UpdateStock(List<StoreStock> storeStock, string barcode){
		await _repo.UpdateStock(storeStock, barcode);
	}
}