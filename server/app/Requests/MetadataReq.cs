namespace app.Requests;

public record MetadataReq{
	public required string Barcode{ get; init; }
	public required decimal Price{ get; init; }
	// public required ICollection<bool> AvailableAt{ get; init; } = [];
}