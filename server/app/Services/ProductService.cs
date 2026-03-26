using app.Helper;
using app.Models;
using app.Repositories;
using app.Requests;

namespace app.Services;

public interface IProductService{
	Task<List<Product>> FetchProducts(AddProductReq request);
	Task<List<Product>> GetAll();
}

public class ProductService: IProductService{
	private readonly IProductRepository _productRepository;

	public ProductService(IProductRepository productRepository){
		_productRepository = productRepository;
	}

	public async Task<List<Product>> FetchProducts(AddProductReq request){
		var product = await Discogs.CreateProduct(request.Code, request.IsBarcode, request.Price);
		return product;
	}

	public async Task<List<Product>> GetAll(){
		return await _productRepository.GetAllAsync();
	}
}