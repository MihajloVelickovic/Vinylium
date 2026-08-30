using System.Globalization;
using app.Models;
using app.Repositories;
using app.Requests;

namespace app.Services;

public interface IStoreService{
	Task<Store> CreateStoreAsync(AddStoreReq req);
	Task DeleteStoreAsync(int id);
	Task<List<Store>?> GetAllStoresAsync();
	Task<Store> GetStoreByIdAsync(int id);
	Task<Store> UpdateStoreAsync(UpdateStoreReq req);
	Task<List<Store>?> GetStoresAsync();
	public Task<bool> HasWarehouse();
	Task<(List<Store> result, int pages)> GetFilteredAsync(int? page, int? items, string? search, bool? isWarehouse);
}

public class StoreService: IStoreService{
	
	private readonly IStoreRepository _storeRepository;
	private readonly IStoreStockService _storeStockService;
	private readonly IUnitOfWork _unitOfWork;

	public StoreService(IStoreRepository storeRepository, IStoreStockService storeStockService, IUnitOfWork unitOfWork){
		_storeRepository = storeRepository;
		_storeStockService = storeStockService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Store> CreateStoreAsync(AddStoreReq req){
		var parsedOt = TimeOnly.FromDateTime(DateTime.ParseExact(req.OpeningHours, "HH:mm", CultureInfo.InvariantCulture));
		var parsedCt = TimeOnly.FromDateTime(DateTime.ParseExact(req.ClosingHours, "HH:mm", CultureInfo.InvariantCulture));
		var store = new Store{
			Address = req.Address,
			City = req.City,
			ContactNumber = req.ContactNumber,
			Name = req.Name + (req.IsWarehouse ? " Warehouse" : ""),
			OpeningTime = parsedOt,
			ClosingTime = parsedCt,
			IsWarehouse = req.IsWarehouse
		};

		await _unitOfWork.ExecuteInTransactionAsync(async () => {
			var s = await _storeRepository.CreateStoreAsync(store);
			await _storeStockService.CreateStockForNewStore(s.Id);
		});
		
		return store;
	}

	public async Task<bool> HasWarehouse(){
		return await _storeRepository.HasWarehouse();
	}
	
	public async Task DeleteStoreAsync(int id){
		var store = await GetStoreByIdAsync(id);
		int? wh;
		await _unitOfWork.ExecuteInTransactionAsync(async () => {
			if(!store.IsWarehouse && (wh = await GetWarehouseId()) != null)
				await _storeStockService.MoveToWarehouse(id, wh.Value);
			await _storeRepository.DeleteStoreAsync(id);
		});
		
	}

	private async Task<int?> GetWarehouseId(){
		return await _storeRepository.GetWarehouseId();
	}

	public async Task<List<Store>?> GetAllStoresAsync(){
		return await _storeRepository.GetAllStoresAsync();
	}

	public async Task<Store> GetStoreByIdAsync(int id)
	{
		return await _storeRepository.GetStoreByIdAsync(id);
	}

	public async Task<Store> UpdateStoreAsync(UpdateStoreReq req)
	{
		var parseddOt =
			TimeOnly.FromDateTime(DateTime.ParseExact(req.OpeningHours, "HH:mm", CultureInfo.InvariantCulture));
		var parsedCt =
			TimeOnly.FromDateTime(DateTime.ParseExact(req.ClosingHours, "HH:mm", CultureInfo.InvariantCulture));
		
		var change = new Store{
			Name = req.Name,
			Address = req.Address,
			City = req.City,
			ContactNumber = req.ContactNumber,
			OpeningTime = parseddOt,
			ClosingTime = parsedCt,
			IsWarehouse = req.IsWarehouse
		};
		return await _storeRepository.UpdateStoreAsync(req.Id, change);
	}

	public async Task<List<Store>?> GetStoresAsync(){
		return await _storeRepository.GetStoresAsync();
	}

	public async Task<(List<Store> result, int pages)> GetFilteredAsync(int? page, int? items, string? search, bool? isWarehouse){
		var filter = new StoreFilterReq{
			Page = page,
			PerPage = items,
			Search = search,
			IsWarehouse = isWarehouse
		};
		return await _storeRepository.GetFilteredAsync(filter);
	}
}