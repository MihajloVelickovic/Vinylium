using app.Models;
using Newtonsoft.Json.Linq;

namespace app.Repositories;

public interface IStoreStockRepository{
	Task CreateStoreStockAsync(List<StoreStock> storeStock);
}

public class StoreStockRepository: IStoreStockRepository{
	private readonly VinyliumContext _dbContext;
	public StoreStockRepository(VinyliumContext context){
		_dbContext = context;		
	}
	
	public async Task CreateStoreStockAsync(List<StoreStock> storeStock){
		await _dbContext.StoreStocks.AddRangeAsync(storeStock);
	}
}