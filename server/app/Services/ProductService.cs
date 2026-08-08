using System.Net.Quic;
using System.Runtime.InteropServices;
using System.Text;
using app.Enums;
using app.Helper;
using app.Models;
using app.Repositories;
using app.Requests;
using Newtonsoft.Json.Linq;

namespace app.Services;

public interface IProductService{
	Task<List<Product>> FetchProducts(AddProductReq request);
	Task<List<Product>> GetAll();
	Task<Product> AddProductAsync(AcceptProductReq req);
	Task<Product> GetByIdAsync(string barcode);
	Task<(List<Product> result, int pages)> GetFilteredAsync(int? page, int? items, string? search, int? type,
		decimal? pL, decimal? pH);
	Task<List<Product>> GetRandomProductsAsync();
	Task<int> GetCount();
	Task<List<Product>> GetPage(int? page, int? items);
	Task<List<Store>> GetAvailableStoresByIdAsync(string barcode);
	Task<bool> ExistsProductId(string barcode);
}

public class ProductService: IProductService{
	private readonly IProductRepository _productRepository;
	private readonly IStoreStockService _storeStockService;
	private readonly IUnitOfWork _unitOfWork;
	public ProductService(IProductRepository productRepository, IStoreStockService storeStockService, IUnitOfWork uow){
		_productRepository = productRepository;
		_storeStockService = storeStockService;
		_unitOfWork = uow;
	}

	public async Task<List<Product>> FetchProducts(AddProductReq request){
		var exists = await ExistsProductId(request.Code);
		if(exists)
			throw new Exception("Product with this barcode is already in the database. Update it instead");
		return await Discogs.CreateProduct(request.Code);
	}

	public async Task<List<Product>> GetAll(){
		return await _productRepository.GetAllAsync();
	}

	private Product GetProductFromJson(object jsonParam){
		/* the frontend sends the entire Product "object" in the request
		 * to make the request itself easier to parse visually
		 * because it's a weird funky object, its type is 'object',
		 * so this checking and casting is needed to make it into
		 * a c# Product object that can be added to the db
		 */
		var productJObjectString = jsonParam.ToString() ??
		                           throw new Exception("Failed to create product string from request data.");
		
		var productJObject = JObject.Parse(productJObjectString);
		
		var product = productJObject.ToObject<Product>() ?? 
		              throw new Exception("Failed to cast json to product.");
		return product;
	}
	
	public async Task<Product> AddProductAsync(AcceptProductReq req){
		var product = this.GetProductFromJson(req.Product);
		var storeStock = _storeStockService.CreateStoreStockFromJson(req.StoreQuantities, product.Barcode);
		/* atomically execute product and store stock creation */
		await _unitOfWork.ExecuteInTransactionAsync(async () => {
			await _productRepository.CreateProductAsync(product);
			await _storeStockService.CreateStoreStock(storeStock);
		});
		return product;
	}

	public async Task<bool> ExistsProductId(string barcode){
		return await _productRepository.ExistsProductId(barcode);
	}

	public async Task<Product> GetByIdAsync(string barcode){
		return await _productRepository.GetByIdAsync(barcode);
	}

	public async Task<(List<Product> result, int pages)> GetFilteredAsync(int? page, int? items, string? search,
		int? type, decimal? pL,
		decimal? pH){
		
		if(type != null && !Enum.IsDefined(typeof(ProductType), type))
			throw new Exception("Type not valid");

		var filter = new FilterReq(){
			Page = page,
			PerPage = items,
			Search = search,
			Type = (ProductType?)type,
			PriceLow = pL,
			PriceHigh = pH
		};

		return await _productRepository.GetFilteredAsync(filter);

	}

	public async Task<List<Product>> GetRandomProductsAsync(){
		return await _productRepository.GetRandomProductsAsync();
	}

	public async Task<int> GetCount(){
		return await _productRepository.GetCount();
	}
	
	public async Task<List<Product>> GetPage(int? page, int? items){
		var p = page ?? 1;

		var i = items ?? throw new Exception("Need to return some items");
		
		return await _productRepository.GetPage(p, i);
	}

	public Task<List<Store>> GetAvailableStoresByIdAsync(string barcode){
		throw new NotImplementedException();
	}
}