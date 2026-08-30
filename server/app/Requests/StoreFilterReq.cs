namespace app.Requests;

public record StoreFilterReq{
	public int? Page {get; init;}
	public int? PerPage { get; init; }
	public string? Search { get; init; }
	public bool? IsWarehouse { get; init; }
}
