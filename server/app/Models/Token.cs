using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class Token{
	[Key] public string Id{ get; init; } = null!;

	public int UserId{ get; init; }

	public User User{ get; set; } = null!;
}