namespace app.Models;

public class CartMergeResult{
	public CartView? Cart{ get; init; }

	public required int MergedItems{ get; init; }

	public required List<string> Capped{ get; init; }

	public required List<string> Dropped{ get; init; }
}
