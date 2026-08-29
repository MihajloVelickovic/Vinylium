using app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Newtonsoft.Json.Linq;

namespace app.Repositories;

public interface IStoreStockRepository{
	Task CreateStoreStockAsync(List<StoreStock> storeStock);
	Task<List<StoreStock>> GetStoreStockFromId(string barcode);
	Task UpdateStock(List<StoreStock> storeStock, string barcode);
	Task MoveToWarehouse(int id, int wh);
	Task CreateStockForNewStore(int storeId);
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
										   .OrderBy(s => s.StoreId)
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

	public async Task MoveToWarehouse(int id, int wh){
		var stockToMove = await _dbContext.StoreStocks
										  .Where(ss => ss.StoreId == id)
										  .ToListAsync();
		
		foreach(var stock in stockToMove){
			var productInWh = await _dbContext.StoreStocks.Where(ss => ss.ProductBarcode == stock.ProductBarcode && 
																 ss.StoreId == wh)
														  .SingleOrDefaultAsync();
			if(productInWh == null)
				continue;
			productInWh.Quantity += stock.Quantity;
			stock.Quantity = 0;
		}
	}

	public async Task CreateStockForNewStore(int storeId){
		var products = await _dbContext.StoreStocks.Select(ss => ss.ProductBarcode)
												   .Distinct()
												   .ToListAsync();

		List<StoreStock> toAdd =[
			.. products.Select(product => new StoreStock{
				StoreId = storeId, 
				ProductBarcode = product, 
				Quantity = 0
			})
		];

		await _dbContext.StoreStocks.AddRangeAsync(toAdd);
		
	}
}