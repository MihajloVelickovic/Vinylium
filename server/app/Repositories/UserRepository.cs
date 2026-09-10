using app.Helper;
using app.Models;
using app.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace app.Repositories;

public interface IUserRepository{
	Task RegisterUserAsync(User user);
	Task<User?> FindUserByEmailOrUsernameAsync(string emailOrUsername);
	Task<User?> FindUserByIdAsync(Guid id);
	Task DeleteUserAsync(string username);
	Task DeleteUserByIdAsync(Guid id);
	Task<User> SetAdminAsync(Guid id, bool admin);
	Task<User> UpdateEmailAsync(Guid id, string email);
	Task<User> UpdatePasswordAsync(Guid id, string hashedPassword);
	Task<bool> IsTokenVersionCurrentAsync(Guid id, int version);
	Task<List<User>> GetAllUsersAsync();
	Task<(List<User> result, int pages)> GetFilteredAsync(UserFilterReq req);
}

public class UserRepository: IUserRepository{
	private readonly VinyliumContext _dbContext;
	private readonly IDistributedCache _cache;

	public UserRepository(VinyliumContext db, IDistributedCache cache){
		_dbContext = db;
		_cache = cache;
	}
	
	private static readonly DistributedCacheEntryOptions VersionCacheOptions = new(){
		AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(double.Parse(DotEnv.Get("JWT_EXP") ?? "30"))
	};

	private async Task CacheTokenVersionAsync(User user){
		try{
			await _cache.SetStringAsync($"user:{user.Id}:ver", user.TokenVersion.ToString(), VersionCacheOptions);
		}
		catch{
			await ForgetTokenVersionAsync(user.Id);
		}
	}

	private async Task ForgetTokenVersionAsync(Guid id){
		try{
			await _cache.RemoveAsync($"user:{id}:ver");
		}
		catch{
			// ignored
		}
	}
	
	public async Task<bool> IsTokenVersionCurrentAsync(Guid id, int version){
		try{
			var cached = await _cache.GetStringAsync($"user:{id}:ver");
			if(int.TryParse(cached, out var cachedVersion) && cachedVersion == version)
				return true;
		}
		catch{
			//ignored
		}

		var user = await FindUserByIdAsync(id);
		if(user == null){
			await ForgetTokenVersionAsync(id);
			return false;
		}

		await CacheTokenVersionAsync(user);
		return user.TokenVersion == version;
	}

	public async Task RegisterUserAsync(User user){
		var sameUsername = await FindUserByEmailOrUsernameAsync(user.Username);
		if(sameUsername != null)
			throw new Exception($"Username {user.Username} is already taken");

		var sameEmail = await FindUserByEmailOrUsernameAsync(user.Email);
		if(sameEmail != null)
			throw new Exception($"Email {user.Email} is already taken");

		_ = await _dbContext.Users.AddAsync(user) ??
		    throw new Exception($"User {user.Username} Already Exists");
		/* this exception above should never happen because of previous checks
		 * we already have (email, username) pairs as a unique index in the db
		 * but these checks can provide more detailed error messages (hopefully)
		 */

		var writes = await _dbContext.SaveChangesAsync();
		if(writes < 0)
			throw new Exception("Failed to write user to database");
	}

	public async Task<User?> FindUserByEmailOrUsernameAsync(string emailOrUsername){
		return await _dbContext.Users.Where(u => u.Email == emailOrUsername || u.Username == emailOrUsername)
			.SingleOrDefaultAsync();
	}

	public async Task<User?> FindUserByIdAsync(Guid id){
		return await _dbContext.Users.Where(u => u.Id == id).SingleOrDefaultAsync();
	}

	public async Task DeleteUserAsync(string username){
		var user = await FindUserByEmailOrUsernameAsync(username);
		if(user == null)
			throw new Exception($"User {username} not found");
		await RemoveUserAsync(user);
	}

	public async Task DeleteUserByIdAsync(Guid id){
		var user = await FindUserByIdAsync(id) ??
		           throw new Exception($"User with id {id} not found");
		await RemoveUserAsync(user);
	}

	private async Task RemoveUserAsync(User user){
		var orders = await _dbContext.Orders.Where(o => o.UserId == user.Id).ToListAsync();
		foreach(var order in orders)
			order.UserId = null;

		_dbContext.Remove(user);
		await _dbContext.SaveChangesAsync();
		await ForgetTokenVersionAsync(user.Id);
	}

	public async Task<User> SetAdminAsync(Guid id, bool admin){
		var user = await FindUserByIdAsync(id) ??
		           throw new Exception($"User with id {id} not found");
		user.Admin = admin;
		user.TokenVersion++;
		await _dbContext.SaveChangesAsync();
		await CacheTokenVersionAsync(user);
		return user;
	}

	public async Task<User> UpdateEmailAsync(Guid id, string email){
		var user = await FindUserByIdAsync(id) ??
		           throw new Exception($"User with id {id} not found");
		
		var sameEmail = await FindUserByEmailOrUsernameAsync(email);
		if(sameEmail != null && sameEmail.Id != id)
			throw new Exception($"Email {email} is already taken");

		user.Email = email;
		user.TokenVersion++;
		await _dbContext.SaveChangesAsync();
		await CacheTokenVersionAsync(user);
		return user;
	}

	public async Task<User> UpdatePasswordAsync(Guid id, string hashedPassword){
		var user = await FindUserByIdAsync(id) ??
		           throw new Exception($"User with id {id} not found");

		user.Password = hashedPassword;
		user.TokenVersion++;
		await _dbContext.SaveChangesAsync();
		await CacheTokenVersionAsync(user);
		return user;
	}

	public async Task<List<User>> GetAllUsersAsync(){
		return await _dbContext.Users.ToListAsync();
	}

	public async Task<(List<User> result, int pages)> GetFilteredAsync(UserFilterReq req){

		var query = _dbContext.Users.AsQueryable();

		var page = req.Page ?? 1;
		var perPage = req.PerPage ?? 20;
		var skip = (page - 1) * perPage;

		if(!string.IsNullOrWhiteSpace(req.Search))
			query = query.Where(u => EF.Functions.ILike(u.Username, $"%{req.Search}%") ||
			                         EF.Functions.ILike(u.Email, $"%{req.Search}%"));

		if(req.Admin != null)
			query = query.Where(u => u.Admin == req.Admin);

		var totalCount = await query.CountAsync();
		var pages = totalCount / perPage + 1;
		var result = await query.OrderBy(u => u.Id).Skip(skip).Take(perPage).ToListAsync();
		return (result, pages);

	}
}