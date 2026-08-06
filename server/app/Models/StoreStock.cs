namespace app.Models;

public class StoreStock{
	public int StoreId{ get; init; }
	public Store Store{ get; init; } = null!;
	
	public int ProductId{ get; init; }
	public Product Product{ get; init; } = null!;
	
	public int Quantity{ get; set; }
}