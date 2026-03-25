namespace app.Requests;

public record RefreshTokenReq{
	public required string RefreshToken { get; init; }
}