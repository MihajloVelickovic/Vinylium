namespace app.Models;

public class Vinylium{
    public int Id { get; init; }
    public int WarehouseId { get; init; }
    public Warehouse Warehouse { get; init; } = null!;
    public ICollection<User> Users { get; } = [];
    public ICollection<Store> Stores { get; } = [];
}