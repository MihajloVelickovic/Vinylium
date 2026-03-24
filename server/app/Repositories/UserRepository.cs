using app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace app.Repositories;

public interface IUserRepository{
	Task RegisterUserAsync(User user);
	Task<User?> FindUserByEmailOrUsernameAsync(string emailOrUsername);

}

public class UserRepository:  IUserRepository{
	
	private readonly VinyliumContext _dbContext;
	
	public UserRepository(VinyliumContext db){
		_dbContext = db;
	}

	public async Task RegisterUserAsync(User user){
	
		var sameUsername = await FindUserByEmailOrUsernameAsync(user.Username);
		if(sameUsername != null)
			throw new Exception($"Username {user.Username} is already taken");
		
		var sameEmail = await FindUserByEmailOrUsernameAsync(user.Email);
		if(sameEmail != null)
			throw new Exception($"Email {user.Email} is already taken");
		
		_ = await _dbContext.Users.AddAsync(user) ??
		    throw new Exception($"User Already Exists"); 
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
}	