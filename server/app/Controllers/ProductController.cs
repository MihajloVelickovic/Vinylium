using app.Helper;
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
	
	// TODO CRUD

	[HttpPost("AddProduct")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> AddProduct([FromBody] AddProductReq request){
		try{
			var product = await Discogs.CreateProduct(request.Code, request.IsBarcode, request.Price);
			return Ok(new{data=product});
		}
		catch(Exception e){
			return BadRequest(e.Message);
		}
	}
	
	
	//public async Task<ActionResult> GetProductById()
	
	
	
}