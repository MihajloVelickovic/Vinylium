using app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Newtonsoft.Json.Linq;

namespace app.Repositories;

public interface IStoreStockRepository{
	Task CreateStoreStockAsync(List<StoreStock> storeStock);
	Task<List<StoreStock>> GetStoreStockFromId(string barcode);
	Task UpdateStock(List<StoreStock> storeStock, string barcode);
	Task MoveToWarehouse(Guid id, Guid wh);
	Task CreateStockForNewStore(Guid storeId, List<string> barcodes);
	Task DecrementStockAsync(Guid storeId, string barcode, int quantity);
	Task IncrementStockAsync(Guid storeId, string barcode, int quantity);
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

	public async Task MoveToWarehouse(Guid id, Guid wh){
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

	public async Task CreateStockForNewStore(Guid storeId, List<string> barcodes){
		List<StoreStock> toAdd =[
			.. barcodes.Select(barcode => new StoreStock{
				StoreId = storeId,
				ProductBarcode = barcode,
				Quantity = 0
			})
		];
		await _dbContext.StoreStocks.AddRangeAsync(toAdd);
	}

	public async Task DecrementStockAsync(Guid storeId, string barcode, int quantity){
		var stock = await _dbContext.StoreStocks
									.FirstOrDefaultAsync(ss => ss.StoreId == storeId && ss.ProductBarcode == barcode) ??
					throw new Exception($"No stock record for store {storeId} / product {barcode}");

		if(stock.Quantity < quantity)
			throw new Exception($"Insufficient stock for {barcode} at the selected store");

		stock.Quantity -= quantity;
	}

	public async Task IncrementStockAsync(Guid storeId, string barcode, int quantity){
		var stock = await _dbContext.StoreStocks
									.FirstOrDefaultAsync(ss => ss.StoreId == storeId && ss.ProductBarcode == barcode) ??
					throw new Exception($"No stock record for store {storeId} / product {barcode}");

		stock.Quantity += quantity;
	}
}