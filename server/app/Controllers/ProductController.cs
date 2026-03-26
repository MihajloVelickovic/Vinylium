using app.Models;
using app.Requests;
using app.Services;
using Microsoft.AspNetCore.Mvc;

namespace app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController: ControllerBase{
	private readonly IProductService _productService;

	public ProductController(IProductService productService){
		_productService = productService;
	}

	[HttpGet("FetchProducts")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> FetchProducts([FromBody] AddProductReq request){
		try{
			if(string.IsNullOrWhiteSpace(request.Code))
				throw new ArgumentNullException(nameof(request.Code));

			var product = await _productService.FetchProducts(request);
			return Ok(new{ data = product });
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	[HttpGet("GetAllProducts")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetAllProducts(){
		try{
			var list = await _productService.GetAll();
			return Ok(new{ data = list });
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}

	//Todo filtered get
	[HttpGet("GetProductsFiltered")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> GetProductsFiltered([FromBody] FilterReq req){
		return Ok();
	}
}