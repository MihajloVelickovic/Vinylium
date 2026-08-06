namespace app.Requests;

public record AddStoreReq{
	public required string Name{ get; init; }
	public required string Address{ get; init; }
	//^(\+381|0)[1-9]\d{7,8}$
	public required string ContactNumber{ get; init; }
}