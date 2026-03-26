using app.Models /**/;
using app.Repositories;
using app.Requests;
using BC = BCrypt.Net.BCrypt;

namespace app.Services;

public interface IUserService{
	Task<User> RegisterUserAsync(RegisterReq req);
	Task<User> LoginUserAsync(LoginReq request);
	Task DeleteUserAsync(string username);
	Task<User?> FindUserByEmailOrUsernameAsync(string username);
}

public class UserService: IUserService{
	private readonly IUserRepository _userRepository;

	public UserService(IUserRepository repository){
		_userRepository = repository;
	}

	public async Task<User> RegisterUserAsync(RegisterReq req){
		var hashedPassword = BC.HashPassword(req.Password, salt: BC.GenerateSalt());

		var user = new User{
			Email = req.Email,
			Username = req.Username,
			Password = hashedPassword,
			Admin = false
		};

		await _userRepository.RegisterUserAsync(user);
		return user;
	}

	public async Task<User?> FindUserByEmailOrUsernameAsync(string username){
		return await _userRepository.FindUserByEmailOrUsernameAsync(username);
	}

	public async Task<User> LoginUserAsync(LoginReq request){
		var user = await _userRepository.FindUserByEmailOrUsernameAsync(request.EmailOrUsername) ??
		           throw new Exception($"User {request.EmailOrUsername} does not exist");

		var correctPassword = BC.Verify(request.Password, user.Password);
		return !correctPassword ? throw new Exception("Incorrect password") : user;
	}

	public async Task DeleteUserAsync(string username){
		await _userRepository.DeleteUserAsync(username);
	}
}