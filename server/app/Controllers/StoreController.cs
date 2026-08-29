using System.Globalization;
using app.Helper;
using app.Models;
using app.Requests;
using app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoreController: ControllerBase{
	
	private readonly IStoreService _storeService;
	public StoreController(IStoreService storeService){
		_storeService = storeService;
	}

    //both creating and editing endpoints are doing the same validation so we now just have a function that they call instead
	private static void ValidateStoreInput(string openingHours, string closingHours, string contactNumber){
		var parsedOt = DateTime.TryParseExact(openingHours, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

		if(!RegExp.Check(@"^[0-9]{2}:[0-9]{2}$", openingHours) || !parsedOt)
			throw new Exception("Opening hours not following valid 00:00 to 24:00 time format");

		var parsedCt = DateTime.TryParseExact(closingHours, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

		if(!RegExp.Check(@"^[0-9]{2}:[0-9]{2}$", closingHours) ||  !parsedCt)
			throw new Exception("Closing hours not following valid 00:00 to 24:00 time format");

		if(!RegExp.Check(@"^(\+381|0)[1-9]\d{7,8}$", contactNumber))
			throw new Exception("Contact number not valid Serbian phone number");
	}

	[Authorize]
	[HttpPost("CreateStore")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> CreateStore([FromBody] AddStoreReq req){
		try{
			ValidateStoreInput(req.OpeningHours, req.ClosingHours, req.ContactNumber);
			var store = await _storeService.CreateStoreAsync(req);
			return Ok(new{ data = store });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message});
		}	
	}

	[HttpGet("HasWarehouse")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> HasWarehouse(){
		try{
			var wh = await _storeService.HasWarehouse();
			return Ok(new {data = wh});
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message});
		}
	}
	
	[Authorize]
	[HttpDelete("DeleteStore/{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> DeleteStoreById(string id){
		try{
			var x = int.TryParse(id, out var idInt);
			await _storeService.DeleteStoreAsync(idInt);

			return Ok();
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message});
		}
	}
	
	[Authorize]
	[HttpGet("GetAllStores")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetAllStores(){
		try{
			var stores = await _storeService.GetAllStoresAsync();
			return Ok(new {data = stores});
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message});
		}
	}
	
	[HttpGet("GetStores")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetStores(){
		try{
			var stores = await _storeService.GetStoresAsync();
			return Ok(new {data = stores});
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message});
		}
	}

	[Authorize]
	[HttpGet("GetStoreById/{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetStoreById(int id){
		try{
			var store = await _storeService.GetStoreByIdAsync(id);
			return Ok(new{ data = store });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message});
		}
	}

	[Authorize]
	[HttpPut("UpdateStore")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> UpdateStore([FromBody] UpdateStoreReq req){
		try{
			ValidateStoreInput(req.OpeningHours, req.ClosingHours, req.ContactNumber);
			if(req.IsWarehouse && await _storeService.HasWarehouse())
				throw new Exception("Warehouse already exists");
			var store = await _storeService.UpdateStoreAsync(req);
			return Ok(new{ data = store });
		}
		catch(Exception e){
			return BadRequest(new{ message = e.Message});
		}
	}

}
