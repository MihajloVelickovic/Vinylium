using app.Requests;
using app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartController: ControllerBase{
	private readonly ICartService _cartService;

	public CartController(ICartService cartService){
		_cartService = cartService;
	}

	private Guid? CurrentUserId(){
		var idClaim = HttpContext.User?.FindFirst("id");
		return idClaim != null && Guid.TryParse(idClaim.Value, out var parsed) ? parsed : null;
	}

	private async Task<Guid?> ResolveCartIdAsync(Guid? requestCartId){
		var userId = CurrentUserId();
		return userId == null ?
			   requestCartId :
			   await _cartService.GetCartIdForUserAsync(userId.Value);
	}

	[HttpPost("AddItem")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> AddItem([FromBody] AddCartItemReq request){
		try{
			var userId = CurrentUserId();
			var cartId = userId == null ?
						 request.CartId :
						 await _cartService.GetOrCreateCartIdForUserAsync(userId.Value);

			var cart = await _cartService.AddItemAsync(cartId, request.Barcode, request.StoreId, request.Quantity);
			return Ok(new{ data = cart });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	[HttpPut("UpdateItem")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> UpdateItem([FromBody] UpdateCartItemReq request){
		try{
			var cartId = await ResolveCartIdAsync(request.CartId) ??
			             throw new Exception("Cart not found");

			var cart = await _cartService.UpdateItemAsync(cartId, request.Barcode, request.StoreId, request.Quantity);
			return Ok(new{ data = cart });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	[HttpDelete("RemoveItem/{cartId:guid}/{storeId:guid}/{barcode}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> RemoveItem(Guid cartId, Guid storeId, string barcode){
		try{
			var resolved = await ResolveCartIdAsync(cartId) ??
			               throw new Exception("Cart not found");

			var cart = await _cartService.RemoveItemAsync(resolved, barcode, storeId);
			return Ok(new{ data = cart });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	[Authorize]
	[HttpGet("Mine")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetMyCart(){
		try{
			var userId = CurrentUserId() ??
			             throw new Exception("User not found in token");

			var cart = await _cartService.GetCartForUserAsync(userId);
			return Ok(new{ data = cart });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	[Authorize]
	[HttpPost("Merge")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> Merge([FromBody] MergeCartReq request){
		try{
			var userId = CurrentUserId() ??
			             throw new Exception("User not found in token");

			var result = await _cartService.MergeGuestCartAsync(request.GuestCartId, userId);
			return Ok(new{ data = result });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	[HttpGet("{cartId:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetCart(Guid cartId){
		try{
			var cart = await _cartService.GetCartAsync(cartId, CurrentUserId());
			if(cart == null)
				throw new Exception("Cart not found");
			return Ok(new{ data = cart });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}
}
