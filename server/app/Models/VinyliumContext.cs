using Microsoft.EntityFrameworkCore;

namespace app.Models;

public class VinyliumContext: DbContext{
	public DbSet<User> Users{ get; set; } = null!;
	public DbSet<Store> Stores{ get; set; } = null!;
	public DbSet<Product> Products{ get; set; } = null!;
	public DbSet<Token> Tokens{ get; set; } = null!;
	public DbSet<StoreStock> StoreStocks{ get; set; } = null!;
	public VinyliumContext(DbContextOptions options): base(options){}

	protected override void OnModelCreating(ModelBuilder builder){
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

		builder.Entity<Product>()
			.OwnsMany(p => p.Tracklist, t => {
				t.ToJson();
			});

	}
}