using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class Order{
	public Guid Id{ get; init; } = Guid.CreateVersion7();

	public Guid? UserId{ get; set; }

	[StringLength(254)] [EmailAddress] public required string Email{ get; set; }

	public DateTime CreatedAt{ get; init; } = DateTime.UtcNow;

	public ICollection<OrderItem> Items{ get; set; } = [];
}
