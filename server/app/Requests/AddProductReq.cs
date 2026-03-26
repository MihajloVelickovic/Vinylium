namespace app.Requests;

public record AddProductReq{
	public required string Code{ get; init; }
	public required bool IsBarcode{ get; init; }
	public required decimal Price{ get; init; }
	// todo public required ICollection<bool> AvailableAt{ get; init; } = [];
}