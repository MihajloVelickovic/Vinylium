namespace app.Requests;

public record SetAdminReq{
	public required Guid Id{ get; init; }
	public required bool Admin{ get; init; }
}
