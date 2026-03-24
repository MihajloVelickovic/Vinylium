using app.Models;
using app.Repositories;
using app.Services;
using Microsoft.EntityFrameworkCore;

namespace app;

public class Program {
    public static void Main(string[] args){
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        
        var currentDir = Directory.GetCurrentDirectory();
        var dbPath = builder.Configuration["DbPath"]!;
        var fullPath = Path.Combine(currentDir, dbPath);
        
        builder.Services.AddDbContext<VinyliumContext>(options => 
            options.UseSqlite($"Data Source={fullPath}"));
        
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        
        var app = builder.Build();
        
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
        
    }
}
