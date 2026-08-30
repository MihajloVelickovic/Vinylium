namespace app.Requests;

public record CheckoutReq{
	public required Guid CartId{ get; init; }
	public required string Email{ get; init; }
}
