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
}

public class StoreService: IStoreService{
	
	private readonly IStoreRepository _storeRepository;

	public StoreService(IStoreRepository storeRepository){
		_storeRepository = storeRepository;
	}

	public async Task<Store> CreateStoreAsync(AddStoreReq req){
		var parsedOt = TimeOnly.FromDateTime(DateTime.ParseExact(req.OpeningHours, "HH:mm", CultureInfo.InvariantCulture));
		var parsedCt = TimeOnly.FromDateTime(DateTime.ParseExact(req.ClosingHours, "HH:mm", CultureInfo.InvariantCulture));
		var store = new Store{
			Address = req.Address,
			City = req.City,
			ContactNumber = req.ContactNumber,
			Name = req.Name,
			OpeningTime = parsedOt,
			ClosingTime = parsedCt,
			IsWarehouse = req.IsWarehouse
		};

		await _storeRepository.CreateStoreAsync(store);
		return store;
	}

	public async Task DeleteStoreAsync(int id){
		await _storeRepository.DeleteStoreAsync(id);
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
}