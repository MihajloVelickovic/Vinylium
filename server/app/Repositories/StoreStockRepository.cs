using app.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace app.Repositories;

public interface IStoreStockRepository{
	Task CreateStoreStockAsync(List<StoreStock> storeStock);
	Task<List<StoreStock>> GetStoreStockFromId(string barcode);
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
}