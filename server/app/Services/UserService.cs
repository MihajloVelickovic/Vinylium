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
}