using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace app.Models;

public class VinyliumContext: DbContext{

	public DbSet<Warehouse> Warehouses { get; set; } = null!;
	public DbSet<Vinylium> Vinylium { get; set; } = null!;
	public DbSet<User> Users { get; set; } = null!;
	public DbSet<Store> Locations { get; set; } = null!;
	public DbSet<Product> Products { get; set; } = null!;
	public DbSet<Token> Tokens{ get; set; } = null!;

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
		.HasMany(p => p.Wishlist)
		.WithMany();
		
		builder.Entity<Store>()
		.HasMany(p => p.Products)
		.WithMany(s => s.AvailableAt);
		
		builder.Entity<User>()
		.HasIndex(u => new {u.Username, u.Password})
		.IsUnique();

		builder.Entity<Token>()
		.HasOne(u => u.User)
		.WithMany();

	}

}
