using app.Enums;

namespace app.Requests;

public record FilterReq{
	public string? Title { get; init; }
	public string? Artist { get; init; }
	public ProductType? Type { get; init; }
	public decimal? PriceLow { get; init; }
	public decimal? PriceHigh { get; init; }
}