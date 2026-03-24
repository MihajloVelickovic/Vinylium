namespace app.Models;

public class Store{
    
    public int Id{ get; init; }

    public ICollection<Product> Products { get; init; } = [];
    
}