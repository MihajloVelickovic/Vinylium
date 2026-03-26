using app.Helper;
using app.Services;
using app.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController: ControllerBase{
	private readonly IUserService _userService;
	private readonly IJwtService _jwtService;

	public UserController(IUserService userService, IJwtService jwtService){
		_userService = userService;
		_jwtService = jwtService;
	}

	[HttpPost("Register")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> Register([FromBody] RegisterReq request){
		try{
			/* checks if the email address from the DTO
			 * is properly formatted, doesn't check if it's
			 * a real address
			 * source https://regex101.com/r/lHs2R3/1
			 */
			if(!RegExp.Check(@"^[\w\-\.]+@([\w-]+\.)+[\w-]{2,}$", request.Email))
				throw new Exception("Email address format not valid");

			/* checks if the username is validly formatted
			 * can start with any letter from a-z (capitalized or not) or with
			 * an underscore, afterwards accepts numbers and periods as well
			 */
			if(!RegExp.Check(@"^[a-zA-Z_][a-zA-Z0-9_.]*$", request.Username))
				throw new Exception("Username format not valid");

			/* accepts any ascii character, but requires at least
			 * 8 to be inputted
			 * source https://stackoverflow.com/questions/3203190/regex-any-ascii-character
			 */
			if(!RegExp.Check(@"^[ -~]{8,}$", request.Password))
				throw new Exception("Password format not valid");

			var registeredUser = await _userService.RegisterUserAsync(request);
			return Ok(new{ message = "Successfully Registered" });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	[HttpPost("Login")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> LogIn([FromBody] LoginReq request){
		try{
			var requestIsEmail = request.EmailOrUsername.Contains('@');

			if(requestIsEmail){
				if(!RegExp.Check(@"^[\w\-\.]+@([\w-]+\.)+[\w-]{2,}$", request.EmailOrUsername))
					throw new Exception("Email address format not valid");
			}
			else{
				if(!RegExp.Check(@"^[a-zA-Z_][a-zA-Z0-9_.]*$", request.EmailOrUsername))
					throw new Exception("Username format not valid");
			}

			if(!RegExp.Check(@"^[ -~]{8,}$", request.Password))
				throw new Exception("Password format not valid");

			var user = await _userService.LoginUserAsync(request);

			var guid = Guid.NewGuid().ToString();

			var token = _jwtService.GenerateAccessToken(user.Username, user.Email, user.Admin);
			var refreshToken = _jwtService.GenerateRefreshToken(user.Username, user.Id, guid);

			return Ok(new{
				message = "Successfully Logged In",
				token,
				refreshToken = refreshToken.Result,
				user
			});
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	[HttpPost("RefreshAccess")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> RefreshAccessToken([FromBody] RefreshTokenReq req){
		try{
			var username = await _jwtService.GetUsernameFromToken(req.RefreshToken, true);

			if(string.IsNullOrEmpty(username))
				return Unauthorized("Invalid refresh token");

			var user = await _userService.FindUserByEmailOrUsernameAsync(username);

			if(user == null)
				throw new Exception($"User ${username} not found");

			await _jwtService.DeleteRefreshToken(req.RefreshToken);

			var guid = Guid.NewGuid().ToString();

			var newToken = _jwtService.GenerateAccessToken(user.Username, user.Email, user.Admin);
			var newRefreshToken = _jwtService.GenerateRefreshToken(user.Username, user.Id, guid);

			return Ok(new{
				token = newToken,
				refreshToken = newRefreshToken.Result
			});
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	[Authorize]
	[HttpPost("Logout")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult> LogOut(RefreshTokenReq req){
		var validated = await _jwtService.ValidateToken(req.RefreshToken, true);

		if(validated == null)
			return Unauthorized("Invalid refresh token");

		await _jwtService.DeleteRefreshToken(req.RefreshToken);
		return Ok(new{ message = "Deleted Refresh Token" });
	}

	[Authorize]
	[HttpDelete("Delete")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult> Delete(RefreshTokenReq req){
		try{
			var usernameRef = await _jwtService.GetUsernameFromToken(req.RefreshToken, true);

			var claim = HttpContext.User.FindFirst("username");
			if(claim == null)
				throw new Exception("Access token not found in request");

			var username = claim.Value;
			if(string.CompareOrdinal(username, usernameRef) != 0)
				throw new UnauthorizedAccessException("Tokens weren't assigned to same user");

			await _jwtService.DeleteRefreshToken(req.RefreshToken);
			await _userService.DeleteUserAsync(username);
			return Ok(new{ message = $"Deleted User {username}" });
		}
		catch(UnauthorizedAccessException e){
			return Unauthorized(e.Message);
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}
}