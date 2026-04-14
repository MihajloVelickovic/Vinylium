using System.Text;
using app.Helper;
using app.Models;
using app.Repositories;
using app.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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

		var currentDir = Directory.GetCurrentDirectory();
		var dbPath = builder.Configuration["DbPath"]!;
		var fullPath = Path.Combine(currentDir, dbPath);

		builder.Services.AddDbContext<VinyliumContext>(options =>
			options.UseSqlite($"Data Source={fullPath}"));

		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IUserRepository, UserRepository>();
		builder.Services.AddScoped<IProductService, ProductService>();
		builder.Services.AddScoped<IProductRepository, ProductRepository>();
		builder.Services.AddScoped<IJwtService, JwtService>();
		builder.Services.AddScoped<IJwtRepository, JwtRepository>();


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