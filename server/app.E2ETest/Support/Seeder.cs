using Npgsql;
using StackExchange.Redis;

namespace app.E2ETest.Support;

public static class Seeder{
	public static async Task ResetAsync(){
		await RunSeedScriptAsync();
		await FlushCacheAsync();
	}

	private static async Task RunSeedScriptAsync(){
		var script = await File.ReadAllTextAsync(TestPaths.SeedScript);

		await using var connection = new NpgsqlConnection(TestPaths.PostgresConnectionString());

		try{
			await connection.OpenAsync();
		}
		catch(Exception e){
			Assert.Fail("Could not reach Postgres. Start it with " +
			            $"'docker compose up -d vinylium.database'. {e.Message}");
		}

		await using var command = new NpgsqlCommand(script, connection);
		await command.ExecuteNonQueryAsync();
	}
	
	private static async Task FlushCacheAsync(){
		try{
			using var redis = await ConnectionMultiplexer.ConnectAsync(TestPaths.RedisConnectionString());

			foreach(var endpoint in redis.GetEndPoints())
				await redis.GetServer(endpoint).FlushAllDatabasesAsync();
		}
		catch(Exception e){
			Assert.Fail("Could not reach Redis. Start it with " +
			            $"'docker compose up -d vinylium.cache'. {e.Message}");
		}
	}
}
