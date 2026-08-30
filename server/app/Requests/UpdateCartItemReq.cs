namespace app.Requests;

public record UpdateCartItemReq{
	public required Guid CartId{ get; init; }
	public required Guid StoreId{ get; init; }
	public required string Barcode{ get; init; }
	public required int Quantity{ get; init; }
}
