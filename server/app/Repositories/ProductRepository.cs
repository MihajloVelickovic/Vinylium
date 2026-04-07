using app.Models;
using Microsoft.EntityFrameworkCore;

namespace app.Repositories;

public interface IProductRepository{
	Task CreateProductAsync(Product product);
	Task<List<Product>> GetAllAsync();
	Task<Product> GetByIdAsync(string barcode);
}

public class ProductRepository: IProductRepository{
	private readonly VinyliumContext _dbContext;

	public ProductRepository(VinyliumContext dbContext){
		_dbContext = dbContext;
	}

	public async Task CreateProductAsync(Product product){
		var dbProduct = await _dbContext.Products.AddAsync(product) ??
		                throw new Exception("Failed to add product to database");
		var changes = await _dbContext.SaveChangesAsync();
		if(changes == 0)
			throw new Exception("Failed to write to database");
	}

	public async Task<List<Product>> GetAllAsync(){
		return await _dbContext.Products.ToListAsync();
	}

	public async Task<Product> GetByIdAsync(string barcode){
		return await _dbContext.Products.FirstOrDefaultAsync(p => p.Barcode == barcode) ??
		       throw new Exception($"Failed to get product {barcode} from database");
	}
}