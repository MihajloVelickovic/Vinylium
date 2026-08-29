using System.ComponentModel.DataAnnotations;
using app.Enums;

namespace app.Models;

public class Product{
	[Key] public required string Barcode{ get; init; }

	public required string CatalogNumber{ get; init; }
	
	public required string Name{ get; set; }

	public required string Artist{ get; set; }

	public required string ImageUrl{ get; set; }

	public required decimal? Price{ get; set; }

	public required ProductType Type{ get; init; }
	
	public ICollection<Track> Tracklist{ get; set; } = [];

	[StringLength(11)] public required string Runtime{ get; init; }

	[StringLength(10)] public required string ReleaseDate{ get; init; }
	
	public required bool InStock{ get; set; } 
}