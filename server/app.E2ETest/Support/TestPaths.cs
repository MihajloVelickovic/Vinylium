using app.Helper;

namespace app.E2ETest.Support;

public static class TestPaths{
	private static readonly Lazy<string> Root = new(FindRepositoryRoot);

	public static string RepositoryRoot => Root.Value;

	public static string SeedScript =>
		Path.Combine(RepositoryRoot, "server", "app.E2ETest", "Seed", "seed.sql");

	private static string FindRepositoryRoot(){
		var dir = new DirectoryInfo(AppContext.BaseDirectory);

		while(dir is not null){
			if(File.Exists(Path.Combine(dir.FullName, "compose.yaml")))
				return dir.FullName;

			dir = dir.Parent;
		}

		throw new DirectoryNotFoundException(
			$"Could not find the repository root above {AppContext.BaseDirectory}");
	}

	public static void LoadEnvironment(){
		DotEnv.LoadFromFile(Path.Combine(RepositoryRoot, "server", ".env"));
		DotEnv.LoadFromFile(Path.Combine(RepositoryRoot, "server", "db.env"));
	}

	public static string PostgresConnectionString(){
		LoadEnvironment();

		return $"Host={DotEnv.Get("POSTGRES_HOST")};Database={DotEnv.Get("POSTGRES_DB")};" +
		       $"Username={DotEnv.Get("POSTGRES_USER")};Password={DotEnv.Get("POSTGRES_PASSWORD")};";
	}

	public static string RedisConnectionString(){
		LoadEnvironment();

		return $"{DotEnv.Get("REDIS_CS") ?? "localhost:6379"},allowAdmin=true";
	}
}
