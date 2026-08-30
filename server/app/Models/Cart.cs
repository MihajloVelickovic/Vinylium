namespace app.Models;

public class Cart{
	public Guid Id{ get; init; } = Guid.CreateVersion7();

	public ICollection<CartItem> Items{ get; set; } = [];
}
