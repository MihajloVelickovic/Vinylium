using app.Models;
using Microsoft.EntityFrameworkCore;

namespace app.Repositories;

public interface IOrderRepository{
	Task CreateOrderAsync(Order order);
	Task<List<Order>> GetOrdersForUserAsync(Guid userId, string email);
	Task<int> BackfillUserIdByEmailAsync(Guid userId, string email);
	Task<Order?> GetByIdAsync(Guid orderId);
	Task DeleteOrderAsync(Guid orderId);
}

public class OrderRepository: IOrderRepository{
	private readonly VinyliumContext _dbContext;

	public OrderRepository(VinyliumContext dbContext){
		_dbContext = dbContext;
	}

	public async Task CreateOrderAsync(Order order){
		var _ = await _dbContext.Orders.AddAsync(order) ??
		        throw new Exception("Failed to add order to database");
	}

	public async Task<List<Order>> GetOrdersForUserAsync(Guid userId, string email){
		return await _dbContext.Orders
							   .Include(o => o.Items)
							   .Where(o => o.UserId == userId || o.Email == email)
							   .OrderByDescending(o => o.CreatedAt)
							   .ToListAsync();
	}

	public async Task<int> BackfillUserIdByEmailAsync(Guid userId, string email){
		return await _dbContext.Orders
							   .Where(o => o.UserId == null && o.Email == email)
							   .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.UserId, userId));
	}

	public async Task<Order?> GetByIdAsync(Guid orderId){
		return await _dbContext.Orders.Include(o => o.Items)
									  .FirstOrDefaultAsync(o => o.Id == orderId);
	}

	public async Task DeleteOrderAsync(Guid orderId){
		var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
		if(order == null)
			return;

		_dbContext.Orders.Remove(order);
	}
}
