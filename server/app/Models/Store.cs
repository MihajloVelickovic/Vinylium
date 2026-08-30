using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class Store{
	public Guid Id{ get; init; } = Guid.CreateVersion7();
	[StringLength(50)] public required string Name{ get; set; }
	[StringLength(50)] public required string Address{ get; set; }
	[StringLength(50)] public required string City{ get; set; }
	[StringLength(13)] public required string ContactNumber{ get; set; }
	public required TimeOnly OpeningTime{ get; set; }
	public required TimeOnly ClosingTime{ get; set; }
	public required bool IsWarehouse{ get; set; }
}