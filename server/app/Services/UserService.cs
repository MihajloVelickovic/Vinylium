using app.Models /**/;
using app.Repositories;
using app.Requests;
using BC = BCrypt.Net.BCrypt;

namespace app.Services;

public interface IUserService{
	Task<User> RegisterUserAsync(RegisterReq req);
	Task<User> LoginUserAsync(LoginReq request);
	Task DeleteUserAsync(string username);
	Task DeleteUserByIdAsync(Guid id);
	Task<User> SetAdminAsync(Guid id, bool admin);
	Task<User> UpdateEmailAsync(Guid id, string email, string password);
	Task<User> UpdatePasswordAsync(Guid id, string oldPassword, string newPassword);
	Task<User?> FindUserByEmailOrUsernameAsync(string username);
	Task<User?> FindUserByIdAsync(Guid id);
	Task<List<User>> GetAllUsersAsync();
	Task<(List<User> result, int pages)> GetFilteredAsync(int? page, int? items, string? search, bool? admin);
}

public class UserService: IUserService{
	private readonly IUserRepository _userRepository;
	private readonly IOrderService _orderService;

	public UserService(IUserRepository repository, IOrderService orderService){
		_userRepository = repository;
		_orderService = orderService;
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
		
		try{
			await _orderService.BackfillGuestOrdersAsync(user.Id, user.Email);
		}
		catch{
			// ignored
		}

		return user;
	}

	public async Task<User?> FindUserByEmailOrUsernameAsync(string username){
		return await _userRepository.FindUserByEmailOrUsernameAsync(username);
	}

	public async Task<User?> FindUserByIdAsync(Guid id){
		return await _userRepository.FindUserByIdAsync(id);
	}

	public async Task<List<User>> GetAllUsersAsync(){
		return await _userRepository.GetAllUsersAsync();
	}

	public async Task<(List<User> result, int pages)> GetFilteredAsync(int? page, int? items, string? search, bool? admin){
		var filter = new UserFilterReq{
			Page = page,
			PerPage = items,
			Search = search,
			Admin = admin
		};
		return await _userRepository.GetFilteredAsync(filter);
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

	public async Task DeleteUserByIdAsync(Guid id){
		await _userRepository.DeleteUserByIdAsync(id);
	}

	public async Task<User> SetAdminAsync(Guid id, bool admin){
		return await _userRepository.SetAdminAsync(id, admin);
	}

	public async Task<User> UpdateEmailAsync(Guid id, string email, string password){
		var user = await _userRepository.FindUserByIdAsync(id) ??
		           throw new Exception($"User with id {id} not found");

		var correctPassword = BC.Verify(password, user.Password);
		if(!correctPassword)
			throw new Exception("Incorrect password");

		var updated = await _userRepository.UpdateEmailAsync(id, email);
		
		try{
			await _orderService.BackfillGuestOrdersAsync(updated.Id, updated.Email);
		}
		catch{
			// ignored
		}

		return updated;
	}

	public async Task<User> UpdatePasswordAsync(Guid id, string oldPassword, string newPassword){
		var user = await _userRepository.FindUserByIdAsync(id) ??
		           throw new Exception($"User with id {id} not found");

		var correctPassword = BC.Verify(oldPassword, user.Password);
		if(!correctPassword)
			throw new Exception("Incorrect password");

		var hashedPassword = BC.HashPassword(newPassword, salt: BC.GenerateSalt());
		return await _userRepository.UpdatePasswordAsync(id, hashedPassword);
	}
}