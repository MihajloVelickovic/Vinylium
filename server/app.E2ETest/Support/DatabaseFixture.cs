using app.E2ETest.Support;

[SetUpFixture]
public class DatabaseFixture{
	[OneTimeSetUp]
	public Task SeedDatabase() => Seeder.ResetAsync();
}
