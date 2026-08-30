namespace app.Requests;

public record UpdateStoreReq{
	public required Guid Id{ get; init; }
	public required string Name{ get; init; }
	public required string Address{ get; init; }
	public required string City{ get; init; }
	public required string ContactNumber{ get; init; }
	public required string OpeningHours{ get; init; }
	public required string ClosingHours{ get; init; }
	public required bool IsWarehouse{ get; init; }
}
