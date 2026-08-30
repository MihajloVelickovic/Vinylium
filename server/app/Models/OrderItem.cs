namespace app.Models;

public class OrderItem{
	public Guid Id{ get; init; } = Guid.CreateVersion7();

	public Guid OrderId{ get; set; }

	public Guid StoreId{ get; set; }

	public required string ProductBarcode{ get; set; }

	public required int Quantity{ get; set; }

	public required decimal UnitPrice{ get; set; }
}
