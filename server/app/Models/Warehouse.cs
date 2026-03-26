namespace app.Models;

public class Warehouse{
	public int Id{ get; init; }

	public int VinyliumId{ get; init; }

	public Vinylium Vinylium{ get; init; } = null!;

	public ICollection<Product> Products{ get; } = [];
}