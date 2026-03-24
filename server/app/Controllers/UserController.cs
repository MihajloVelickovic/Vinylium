using app.Helper;
using app.Services;
using app.Requests;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController: ControllerBase{
	
	private readonly IUserService _userService;
	public UserController(IUserService service){
		_userService = service;
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
			return Ok(new{message="Successfully Registered"});
			
		}
		catch(Exception e){
			return BadRequest(new{message = e.Message});
		}
		
	}

	[HttpPost("Login")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> Login([FromBody] LoginReq request){

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
			
			return Ok(new{message="Successfully Logged In", user});
			
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}
	
}