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
	private readonly IDistributedCache _cache;
	public StoreController(IStoreService storeService,  IDistributedCache cache){
		_storeService = storeService;
		_cache =  cache;
	}

	[Authorize]
	[HttpPost("CreateStore")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> CreateStore([FromBody] AddStoreReq req){
		try{
			
			var parsedOt = DateTime.TryParseExact(req.OpeningHours, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var openingTime);
			
			if(!RegExp.Check(@"^[0-9]{2}:[0-9]{2}$", req.OpeningHours) || !parsedOt)
				throw new Exception("Opening hours not following valid 00:00 to 24:00 time format");
			
			var parsedCt = DateTime.TryParseExact(req.ClosingHours, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var closingTime);
			
			if(!RegExp.Check(@"^[0-9]{2}:[0-9]{2}$", req.ClosingHours) ||  !parsedCt)
				throw new Exception("Closing hours not following valid 00:00 to 24:00 time format");

			if(!RegExp.Check(@"^(\+381|0)[1-9]\d{7,8}$", req.ContactNumber))
				throw new Exception("Contact number not valid Serbian phone number");
			
			var store = await _storeService.CreateStoreAsync(req);
			return Ok(new{ data = store });
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}	
	}

	[Authorize]
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

	[HttpGet("GetStores")]
	public async Task<ActionResult> GetAllStores(){
		try{
			var stores = await _storeService.GetAllStoresAsync();
			return Ok(new {data = stores});
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}
	
}
