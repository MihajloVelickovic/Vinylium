namespace app.Requests;

public record UpdatePasswordReq{
	public required string OldPassword{ get; init; }
	public required string NewPassword{ get; init; }
}
