using app.Requests;
using app.Services;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartController: ControllerBase{
	private readonly ICartService _cartService;

	public CartController(ICartService cartService){
		_cartService = cartService;
	}

	[HttpPost("AddItem")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> AddItem([FromBody] AddCartItemReq request){
		try{
			var cart = await _cartService.AddItemAsync(request.CartId, request.Barcode, request.StoreId, request.Quantity);
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
			var cart = await _cartService.UpdateItemAsync(request.CartId, request.Barcode, request.StoreId, request.Quantity);
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
			var cart = await _cartService.RemoveItemAsync(cartId, barcode, storeId);
			return Ok(new{ data = cart });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}

	[HttpGet("{cartId:guid}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult> GetCart(Guid cartId){
		try{
			var cart = await _cartService.GetCartAsync(cartId);
			if(cart == null)
				throw new Exception("Cart not found");
			return Ok(new{ data = cart });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message });
		}
	}
}
