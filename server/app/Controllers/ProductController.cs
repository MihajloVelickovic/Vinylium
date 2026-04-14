using System.Globalization;
using System.Net;
using app.Enums;
using app.Models;
using app.Requests;
using app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController: ControllerBase{
	private readonly IProductService _productService;

	public ProductController(IProductService productService){
		_productService = productService;
	}

	[Authorize]
	[HttpPost("FetchProducts")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status429TooManyRequests)]
	public async Task<ActionResult> FetchProducts([FromBody] AddProductReq request){
		try{
			if(string.IsNullOrWhiteSpace(request.Code))
				throw new ArgumentNullException(nameof(request.Code));

			var product = await _productService.FetchProducts(request);
			return Ok(new{ data = product });
		}
		catch(BadHttpRequestException r){
			if(r.StatusCode == 429)
				return BadRequest($"Too Many Requests to Discogs API. Either the code {request.Code} " +
				                  $"is not unique enough, or too many requests have been made in a short + " +
				                  $"amount of time, in which case you should wait a bit before fetching again!");
			throw;
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	[HttpGet("GetPage")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetPage([FromQuery] int? page, [FromQuery] int? items){
		try{
			var totalCount = await _productService.GetCount();
			var pages = totalCount/(items+1) + 1;		
			var list = await _productService.GetPage(page, items);
			//var list = await _productService.GetAll(page, items);
			return Ok(new{pages, data = list });
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	[HttpGet("GetRandomProducts")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetRandomProducts(){
		try{
			var random = await _productService.GetRandomProductsAsync();
			return Ok(new{data=random});
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	[HttpGet("GetProductsFiltered")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetProductsFiltered([FromQuery] int? page, 
														[FromQuery] int? items,
														[FromQuery] string? search,  
														[FromQuery] int? type, 
														[FromQuery] string? priceLow, 
														[FromQuery] string? priceHigh){
		try{
			
			var pL = priceLow != null ? 
					(decimal.TryParse(priceLow, out var temp) ? temp : (decimal?)null) ??  
			         throw new Exception("Price Low is not a decimal value") :
					(decimal?)null;
			
			var pH = priceHigh != null ? 
					(decimal.TryParse(priceHigh, out temp) ? temp : (decimal?)null) ??  
					throw new Exception("Price High is not a decimal value") :
					(decimal?)null;

			var filtered = await _productService.GetFilteredAsync(page, items, search, type, pL, pH);
			
			return Ok(new{pages=filtered.pages, data=filtered.result});
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}
	
	[Authorize]
	[HttpPost("AddProduct")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> AddProduct([FromBody] AcceptProductReq req){
		try{
			var product = await _productService.AddProductAsync(req);
			return Ok(new{ data = product });
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	[HttpGet("GetProductById/{barcode}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetProductById(string barcode){
		try{
			var product = await _productService.GetByIdAsync(barcode);
			return Ok(new{ data = product });
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}
	
}