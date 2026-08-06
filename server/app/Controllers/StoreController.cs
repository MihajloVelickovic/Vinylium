using app.Requests;
using app.Services;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoreController: ControllerBase{
	
	private readonly IStoreService _storeService;

	public StoreController(IStoreService storeService){
		_storeService = storeService;
	}

	[HttpPost("CreateStore")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> CreateStore([FromBody] AddStoreReq req){
		try{
			var store = await _storeService.CreateStoreAsync(req);
			return Ok(new{ data = store });
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}	
	}

	[HttpDelete("DeleteStore/{id}")]
	public async Task<ActionResult> DeleteStoreById(string id){
		try{
			var x = int.TryParse(id, out var idInt);
			await _storeService.DeleteStoreAsync(idInt);

			return Ok();
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}
	
}