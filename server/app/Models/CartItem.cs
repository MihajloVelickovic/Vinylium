namespace app.Models;

public class CartItem{
	public Guid Id{ get; init; } = Guid.CreateVersion7();

	public Guid CartId{ get; set; }

	public Guid StoreId{ get; set; }

	public required string ProductBarcode{ get; set; }

	public required int Quantity{ get; set; }
}
