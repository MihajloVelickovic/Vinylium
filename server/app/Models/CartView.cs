namespace app.Models;

public class CartView{
	public required Guid Id{ get; init; }
	public required List<CartItemView> Items{ get; init; }
}
