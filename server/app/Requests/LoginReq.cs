namespace app.Requests;

public record LoginReq{
	public required string EmailOrUsername{ get; init; }
	public required string Password{ get; init; }
}