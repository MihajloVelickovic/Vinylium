using app.Repositories;

namespace app.Services;

public interface IProductService{

}

public class ProductService: IProductService{
	
	private readonly IProductRepository _productRepository;
	
	public ProductService(IProductRepository productRepository){
		_productRepository = productRepository;
	}

}