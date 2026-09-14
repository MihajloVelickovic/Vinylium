using System.Text;
using app.Helper;
using app.Models;
using app.Repositories;
using app.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

namespace app;

public class Program{
	public static void Main(string[] args){
		var builder = WebApplication.CreateBuilder(args);
		DotEnv.LoadFromFile("../.env");
		/* checks for discogs api key and secret
		 * still works if they're not set, just with a
		 * smaller rate limit
		 */
		var discogsKey = DotEnv.Get("DISCOGS_KEY");
		var discogsSecret = DotEnv.Get("DISCOGS_SECRET");
		Discogs.Authorize(discogsKey, discogsSecret);

		builder.Services.AddControllers();

		var connectionString = $"Host={DotEnv.Get("POSTGRES_HOST")};Port={DotEnv.Get("POSTGRES_PORT")};Database={DotEnv.Get("POSTGRES_DB")};" +
		                       $"User Id={DotEnv.Get("POSTGRES_USER")};Password={DotEnv.Get("POSTGRES_PASSWORD")};";

		builder.Services.AddDbContext<VinyliumContext>(options =>
			options.UseNpgsql(connectionString, o => o.EnableRetryOnFailure())
		);

		builder.Services.AddStackExchangeRedisCache(options => {
			options.Configuration = $"{DotEnv.Get("REDIS_HOST")}:{DotEnv.Get("REDIS_PORT")}";
		});
		
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IUserRepository, UserRepository>();
		builder.Services.AddScoped<IProductService, ProductService>();
		builder.Services.AddScoped<IProductRepository, ProductRepository>();
		builder.Services.AddScoped<IStoreService, StoreService>();
		builder.Services.AddScoped<IStoreRepository, StoreRepository>();
		builder.Services.AddScoped<IJwtService, JwtService>();
		builder.Services.AddScoped<IJwtRepository, JwtRepository>();
		builder.Services.AddScoped<IStoreStockService, StoreStockService>();
		builder.Services.AddScoped<IStoreStockRepository, StoreStockRepository>();
		builder.Services.AddScoped<ICartService, CartService>();
		builder.Services.AddScoped<ICartRepository, CartRepository>();
		builder.Services.AddScoped<IOrderService, OrderService>();
		builder.Services.AddScoped<IOrderRepository, OrderRepository>();
		builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
		
		builder.Services.AddCors(options => {
			options.AddPolicy("AllowReact",
				policy => {
					policy.WithOrigins("*") // Front end 
						  .AllowAnyHeader()
						  .AllowAnyMethod();
					      //.AllowCredentials();
				});
		});

		var secret = DotEnv.Get("JWT_SECRET") ??
		             throw new InvalidOperationException("JWT_SECRET not found in environment");

		var key = Encoding.ASCII.GetBytes(secret);

		builder.Services.AddAuthentication(options => {
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options => {
				options.RequireHttpsMetadata = false;
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false,
					ClockSkew = TimeSpan.Zero
				};
				options.Events = new JwtBearerEvents{
					OnTokenValidated = async ctx => {
						var idClaim = ctx.Principal?.FindFirst("id")?.Value;
						var verClaim = ctx.Principal?.FindFirst("ver")?.Value;

						if(!Guid.TryParse(idClaim, out var userId) || !int.TryParse(verClaim, out var version)){
							ctx.Fail("Token missing id or version");
							return;
						}

						var users = ctx.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

						if(!await users.IsTokenVersionCurrentAsync(userId, version))
							ctx.Fail("Token no longer valid");
					}
				};
			});

		var app = builder.Build();

		app.UseRouting();
		app.UseCors("AllowReact");
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers();
		app.Run();
	}
}