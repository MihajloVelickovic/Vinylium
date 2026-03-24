using app.Enums;

namespace app.Models;

public class Product{
    public int Id { get; init; }
    public required string Name;
    public required string Artist;
    public required decimal Price { get; set; }
    public required ProductType Type { get; init; }
	public ICollection<string> Tracklist { get; init; } = [];
	public required string Runtime { get; init; }
	public required string ReleaseDate { get; init; }
	public ICollection<Store> AvailableAt { get; } = []; 
    public required bool InWarehouse { get; set; }
}