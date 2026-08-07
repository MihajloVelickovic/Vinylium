namespace app.Requests;

public record AddStoreReq{
	public required string Name{ get; init; }
	public required string Address{ get; init; }
	public required string City{ get; init; }
	public required string ContactNumber{ get; init; }
	public required string OpeningHours{ get; init; }
	public required string ClosingHours{ get; init; }
}