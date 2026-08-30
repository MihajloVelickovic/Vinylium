using System.Security.Claims;
using app.Helper;
using app.Requests;
using app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController: ControllerBase{
	private readonly IOrderService _orderService;

	public OrderController(IOrderService orderService){
		_orderService = orderService;
	}

	[HttpPost("Checkout")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> Checkout([FromBody] CheckoutReq request){
		try{
			if(!RegExp.Check(@"^[\w\-\.]+@([\w-]+\.)+[\w-]{2,}$", request.Email))
				throw new Exception("Email address format not valid");
			
			Guid? userId = null;
			var idClaim = HttpContext.User?.FindFirst("id");
			if(idClaim != null && Guid.TryParse(idClaim.Value, out var parsed))
				userId = parsed;

			var order = await _orderService.CheckoutAsync(request, userId);
			return Ok(new{ message = "Order placed", order });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	[Authorize]
	[HttpGet("MyOrders")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> MyOrders(){
		try{
			var (userId, email) = GetCurrentUser();
			var orders = await _orderService.GetMyOrdersAsync(userId, email);
			return Ok(new{ data = orders });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	[Authorize]
	[HttpDelete("Cancel/{orderId:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> Cancel(Guid orderId){
		try{
			var (userId, email) = GetCurrentUser();
			await _orderService.CancelOrderAsync(orderId, userId, email);
			return Ok(new{ message = "Order cancelled" });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	private (Guid userId, string email) GetCurrentUser(){
		var idClaim = HttpContext.User.FindFirst("id") ??
		              throw new Exception("User id not found in token");
		
		var email = HttpContext.User.FindFirst("email")?.Value ??
		            HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ??
		            throw new Exception("Email not found in token");

		return (Guid.Parse(idClaim.Value), email);
	}
}
