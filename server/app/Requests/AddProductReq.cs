namespace app.Requests;

public record AddProductReq{
	public required string Barcode{ get; init; }
	public required decimal Price{ get; init; }
	// todo public required ICollection<bool> AvailableAt{ get; init; } = [];
}