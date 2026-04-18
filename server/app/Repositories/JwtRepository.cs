using Microsoft.EntityFrameworkCore;
using app.Models;

namespace app.Repositories;

//TODO replace the JWT table in sqlite with some in memory solution

public interface IJwtRepository{
	public Task StoreJtiAsync(string jti, int userId);
	public Task DeleteJtiAsync(string jti);
	public Task FindJtiAndDeleteAsync(string jti);
	public Task<Token> FindJtiAsync(string jti);
}

public class JwtRepository: IJwtRepository{
	private readonly VinyliumContext _dbContext;

	public JwtRepository(VinyliumContext context){
		_dbContext = context;
	}

	public async Task StoreJtiAsync(string jti, int userId){
		/* jti is guid
		 * guids can be assumed to always be unique
		 */
		await _dbContext.AddAsync<Token>(new(){ Id = jti, UserId = userId });
		await _dbContext.SaveChangesAsync();
	}

	public async Task DeleteJtiAsync(string jti){
		_dbContext.Remove<Token>(new(){ Id = jti });
		await _dbContext.SaveChangesAsync();
	}

	public async Task<Token> FindJtiAsync(string jti){
		var exists = await _dbContext.Tokens
			             .Where(tok => tok.Id == jti)
			             .SingleOrDefaultAsync() ??
		             throw new Exception("Temp exception");

		return exists;
	}

	public async Task FindJtiAndDeleteAsync(string jti){
		var exists = await _dbContext.Tokens
			             .Where(tok => tok.Id == jti)
			             .SingleOrDefaultAsync() ??
		             throw new Exception("Temp exception");

		_dbContext.Remove(exists);
		await _dbContext.SaveChangesAsync();
	}
}