using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class User{
	public Guid Id{ get; init; } = Guid.CreateVersion7();

	[StringLength(254)] [EmailAddress] public required string Email{ get; set; }

	[StringLength(254)] public required string Username{ get; set; }

	[StringLength(254)] public required string Password{ get; set; }

	public required bool Admin{ get; set; }

	public ICollection<Product> Cart{ get; } = [];
}