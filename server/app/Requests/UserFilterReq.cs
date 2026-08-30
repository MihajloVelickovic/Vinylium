namespace app.Requests;

public record UserFilterReq{
	public int? Page {get; init;}
	public int? PerPage { get; init; }
	public string? Search { get; init; }
	public bool? Admin { get; init; }
}
