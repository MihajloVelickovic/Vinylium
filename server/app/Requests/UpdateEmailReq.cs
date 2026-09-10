namespace app.Requests;

public record UpdateEmailReq{
	public required string Email{ get; init; }
	public required string Password{ get; init; }
}
