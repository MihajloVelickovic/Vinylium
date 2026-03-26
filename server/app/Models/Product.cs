using System.ComponentModel.DataAnnotations;
using app.Enums;

namespace app.Models;

public class Product{
	
    public int Id { get; init; }

    public required string Name{ get; set; }

    public required string Artist{ get; set; }
    
    public required string ImageUrl{ get; set; }

    public required decimal Price { get; set; }
    
    public required ProductType Type { get; init; }
	
    public ICollection<string> Tracklist { get; init; } = [];
	
    [StringLength(11)]
    public required string Runtime { get; init; }
	
    [StringLength(10)]
    public required string ReleaseDate { get; init; }
	
    public ICollection<Store> AvailableAt { get; } = []; 
    
    public required bool InWarehouse { get; set; }
}