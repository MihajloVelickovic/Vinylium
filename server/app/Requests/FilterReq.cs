using app.Enums;

namespace app.Requests;

public record FilterReq{
	public int? Page {get; init;}
	public int? PerPage { get; init; }
	public string? Search { get; init; }
	public ProductType? Type { get; init; }
	public decimal? PriceLow { get; init; }
	public decimal? PriceHigh { get; init; }
}