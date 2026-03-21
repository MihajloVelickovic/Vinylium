using Microsoft.EntityFrameworkCore;

namespace app.Models;

public class VinyliumContext: DbContext{

	public VinyliumContext(DbContextOptions options): base(options){}

}