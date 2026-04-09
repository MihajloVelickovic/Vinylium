namespace app.Requests;

public record AddProductReq{
	public required string Code{ get; init; }
}