using app.Models;
using Microsoft.EntityFrameworkCore;

namespace app.Repositories;

public interface IUnitOfWork{
	Task ExecuteInTransactionAsync(Func<Task> action);
}

public class UnitOfWork: IUnitOfWork{
	private readonly VinyliumContext _dbContext;

	public UnitOfWork(VinyliumContext context){
		_dbContext = context;
	}
	
	public async Task ExecuteInTransactionAsync(Func<Task> action)
	{
		var strategy = _dbContext.Database.CreateExecutionStrategy();
		await strategy.ExecuteAsync(async () => {
			await using var transaction = await _dbContext.Database.BeginTransactionAsync();
			try{
				await action();
				await _dbContext.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch{
				await transaction.RollbackAsync();
				throw;
			}
		});
	}
}