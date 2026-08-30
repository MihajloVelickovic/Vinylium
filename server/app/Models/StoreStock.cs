using System.ComponentModel.DataAnnotations.Schema;

namespace app.Models;

public class StoreStock{
	public Guid Id{ get; init; } = Guid.CreateVersion7();
	public Guid StoreId{ get; set; }
	public Store Store{ get; init; } = null!;
	
	public required string ProductBarcode{ get; set; }
	public Product Product{ get; init; } = null!;
	
	public int Quantity{ get; set; }
}