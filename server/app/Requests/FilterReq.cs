using app.Enums;

namespace app.Requests;

public record FilterReq{
	public string? Search { get; init; }
	public ProductType? Type { get; init; }
	public decimal? PriceLow { get; init; }
	public decimal? PriceHigh { get; init; }
}