using app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Newtonsoft.Json.Linq;

namespace app.Repositories;

public interface IStoreStockRepository{
	Task CreateStoreStockAsync(List<StoreStock> storeStock);
	Task<List<StoreStock>> GetStoreStockFromId(string barcode);
	Task UpdateStock(List<StoreStock> storeStock, string barcode);
}

public class StoreStockRepository: IStoreStockRepository{
	private readonly VinyliumContext _dbContext;
	public StoreStockRepository(VinyliumContext context){
		_dbContext = context;		
	}
	
	public async Task CreateStoreStockAsync(List<StoreStock> storeStock){
		await _dbContext.StoreStocks.AddRangeAsync(storeStock);
	}

	public async Task<List<StoreStock>> GetStoreStockFromId(string barcode){
		return await _dbContext.StoreStocks.Include(s => s.Store)
										   .Where(s => s.ProductBarcode == barcode)
										   .ToListAsync();
	}

	public async Task UpdateStock(List<StoreStock> storeStock, string barcode){
		var existingStocks = await _dbContext.StoreStocks
									.Where(ss => ss.ProductBarcode == barcode)
									.ToListAsync();

		foreach (var existing in existingStocks){
			var updated = storeStock.FirstOrDefault(x => x.StoreId == existing.StoreId);
			if (updated != null)
				existing.Quantity = updated.Quantity;
			
		}
	}
}