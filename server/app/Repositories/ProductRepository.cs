using app.Models;
using app.Requests;
using Microsoft.EntityFrameworkCore;

namespace app.Repositories;

public interface IProductRepository{
	Task CreateProductAsync(Product product);
	Task<List<Product>> GetAllAsync();
	Task<Product> GetByIdAsync(string barcode);
	Task<List<Product>> GetFilteredAsync(FilterReq req);
	Task<List<Product>> GetRandomProductsAsync();
}

public class ProductRepository: IProductRepository{
	private readonly VinyliumContext _dbContext;
	private readonly Random _rng = new();
	
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

	public async Task<List<Product>> GetFilteredAsync(FilterReq req){

		var query = _dbContext.Products.AsQueryable();
		
		if(!string.IsNullOrWhiteSpace(req.Title))
			query = query.Where(p => p.Name ==  req.Title);
		
		if(!string.IsNullOrWhiteSpace(req.Artist))
			query = query.Where(p => p.Artist ==  req.Artist);

		if(req.Type != null)
			query = query.Where(p => p.Type ==  req.Type);
		
		if(req.PriceLow != null)
			query = query.Where(p => p.Price >=  req.PriceLow);

		if(req.PriceHigh != null)
			query = query.Where(p => p.Price <=  req.PriceHigh);
		
		return await query.ToListAsync();

	}

	public async Task<List<Product>> GetRandomProductsAsync(){
		return await _dbContext.Products.OrderBy(p => EF.Functions.Random())
										.Take(50)
										.ToListAsync();
	}
}