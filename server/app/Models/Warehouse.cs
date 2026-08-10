namespace app.Models;

public class Warehouse{
	public int Id{ get; init; }
	public ICollection<Product> Products{ get; } = [];
}