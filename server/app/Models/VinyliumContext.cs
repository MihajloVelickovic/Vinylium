using Microsoft.EntityFrameworkCore;

namespace app.Models;

public class VinyliumContext: DbContext{
	public DbSet<Warehouse> Warehouses{ get; set; } = null!;
	public DbSet<Vinylium> Vinylium{ get; set; } = null!;
	public DbSet<User> Users{ get; set; } = null!;
	public DbSet<Store> Stores{ get; set; } = null!;
	public DbSet<Product> Products{ get; set; } = null!;
	public DbSet<Token> Tokens{ get; set; } = null!;
	public DbSet<StoreStock> StoreStocks{ get; set; } = null!;

	public VinyliumContext(DbContextOptions options): base(options){}

	protected override void OnModelCreating(ModelBuilder builder){
		builder.Entity<Warehouse>()
			.HasOne(v => v.Vinylium)
			.WithMany();

		builder.Entity<Warehouse>()
			.HasMany(p => p.Products)
			.WithMany();

		builder.Entity<Vinylium>()
			.HasOne(w => w.Warehouse)
			.WithMany();

		builder.Entity<Vinylium>()
			.HasMany(u => u.Users)
			.WithMany();

		builder.Entity<Vinylium>()
			.HasMany(l => l.Stores)
			.WithMany();

		builder.Entity<User>()
			.HasMany(p => p.Cart)
			.WithMany();
		
		builder.Entity<Token>()
			.HasOne(u => u.User)
			.WithMany();

		builder.Entity<StoreStock>()
			.HasOne(s => s.Store)
			.WithMany();

		builder.Entity<StoreStock>()
			.HasOne(p => p.Product)
			.WithMany();
		builder.Entity<User>()
			.HasIndex(u => new{ u.Username, u.Password })
			.IsUnique();
		
		builder.Entity<Store>()
			.HasIndex(s => s.Name)
			.IsUnique();
		
		builder.Entity<Store>()
			.HasIndex(s => s.ContactNumber)
			.IsUnique();
		
	}
}