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

	public async Task<Product> AddProductAsync(AcceptProductReq req){
		/* the frontend sends the entire Product "object" in the request
		 * to make the request itself easier to parse visually
		 * because it's a weird funky object, its type is 'object',
		 * so this checking and casting is needed to make it into
		 * a c# Product object that can be added to the db
		 */
		var jobjectstring = req.Product.ToString() ??
		                    throw new Exception("Failed to create product string from request data.");
		
		var jobject = JObject.Parse(jobjectstring);
		
		var product = jobject.ToObject<Product>() ?? 
		              throw new Exception("Failed to cast json to product.");
		
		await _productRepository.CreateProductAsync(product);
		return product;
	}

	public async Task<Product> GetByIdAsync(string barcode){
		return await _productRepository.GetByIdAsync(barcode);
	}
}