using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class User{
    
	public int Id { get; init; }
    
    [StringLength(254)]
    [EmailAddress]
    public required string Email { get; set; }
    
    [StringLength(254)]
    public required string Username { get; set; }
	
    [StringLength(254)]
    public required string Password { get; set; }
    
    public required bool Admin { get; set; }
    
    public ICollection<Product> Wishlist { get; } = [];

}