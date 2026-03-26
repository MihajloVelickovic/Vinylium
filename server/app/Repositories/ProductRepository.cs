using app.Models;

namespace app.Repositories;

public interface IProductRepository{
	
}

public class ProductRepository: IProductRepository{

	private readonly VinyliumContext _dbContext;

	public ProductRepository(VinyliumContext dbContext){
		_dbContext = dbContext;
	}

}