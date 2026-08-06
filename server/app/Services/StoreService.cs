using app.Models;
using app.Repositories;
using app.Requests;

namespace app.Services;

public interface IStoreService{
	Task<Store> CreateStoreAsync(AddStoreReq req);
	Task DeleteStoreAsync(int id);
}

public class StoreService: IStoreService{
	
	private readonly IStoreRepository _storeRepository;

	public StoreService(IStoreRepository storeRepository){
		_storeRepository = storeRepository;
	}

	public async Task<Store> CreateStoreAsync(AddStoreReq req){
		var store = new Store{
			Address = req.Address,
			ContactNumber = req.ContactNumber,
			Name = req.Name
		};

		await _storeRepository.CreateStoreAsync(store);
		return store;
	}

	public async Task DeleteStoreAsync(int id){
		await _storeRepository.DeleteStoreAsync(id);
	}
}