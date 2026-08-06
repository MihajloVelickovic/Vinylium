using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class Store{
	public int Id{ get; init; }
	[StringLength(16)] public required string Name{ get; set; }
	[StringLength(50)] public required string Address{ get; set; }
	//^(\+381|0)[1-9]\d{7,8}$
	[StringLength(12)] public required string ContactNumber{ get; set; }
	public ICollection<Product> Products{ get; init; } = [];
}