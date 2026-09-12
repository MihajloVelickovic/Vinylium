namespace app.Requests;

public record MergeCartReq{
	public required Guid GuestCartId{ get; init; }
}
