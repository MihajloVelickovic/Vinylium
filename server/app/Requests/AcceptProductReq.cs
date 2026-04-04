namespace app.Requests;

public record AcceptProductReq
{
    public required object Product { get; init; }
}