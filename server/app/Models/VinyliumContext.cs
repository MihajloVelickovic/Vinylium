using Microsoft.EntityFrameworkCore;

namespace app.Models;

public class VinyliumContext: DbContext{
	public DbSet<User> Users{ get; set; } = null!;
	public DbSet<Store> Stores{ get; set; } = null!;
	public DbSet<Product> Products{ get; set; } = null!;
	public DbSet<Token> Tokens{ get; set; } = null!;
	public DbSet<StoreStock> StoreStocks{ get; set; } = null!;
	public DbSet<Cart> Carts{ get; set; } = null!;
	public DbSet<CartItem> CartItems{ get; set; } = null!;
	public DbSet<Order> Orders{ get; set; } = null!;
	public DbSet<OrderItem> OrderItems{ get; set; } = null!;
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
			.HasIndex(u => u.Username)
			.IsUnique();
		
		builder.Entity<User>()
			.HasIndex(u => u.Email)
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

		builder.Entity<Cart>()
			.HasMany(c => c.Items)
			.WithOne()
			.HasForeignKey(i => i.CartId);

		builder.Entity<CartItem>()
			.HasOne<Product>()
			.WithMany();

		builder.Entity<CartItem>()
			.HasOne<Store>()
			.WithMany();

		builder.Entity<Order>()
			.HasMany(o => o.Items)
			.WithOne();
		
		builder.Entity<Order>()
			.HasOne<User>()
			.WithMany()
			.IsRequired(false);

		builder.Entity<OrderItem>()
			.HasOne<Product>()
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<OrderItem>()
			.HasOne<Store>()
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);

	}
}