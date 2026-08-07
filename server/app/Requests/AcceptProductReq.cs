namespace app.Requests;

public record AcceptProductReq{
	public required object Product{ get; init; }
	public required object StoreQuantities{ get; init; }
}