namespace app.Models;

public class User{
    public int Id { get; init; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public ICollection<Product> Wishlist { get; } = [];

}