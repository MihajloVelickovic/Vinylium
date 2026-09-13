using app.E2ETest.Support;

namespace app.E2ETest.Tests;

[TestFixture]
public class AuthTests: VinyliumPageTest{
	[OneTimeTearDown]
	public Task RestoreBaseline() => Seeder.ResetAsync();

	[Test]
	public async Task RegistrationCreatesAnAccountAndSignsTheUserIn(){
		await OpenAsync("/login");

		await Page.GetByTestId("auth-mode-register").ClickAsync();
		await Page.GetByTestId("auth-identifier").FillAsync("newcomer@vinylium.test");
		await Page.GetByTestId("auth-username").FillAsync("newcomer");
		await Page.GetByTestId("auth-password").FillAsync(TestData.Password);
		await Page.GetByTestId("auth-submit").ClickAsync();

		await Expect(Page.GetByTestId("auth-message")).ToHaveTextAsync("Successful registration");
		await Page.WaitForURLAsync($"{BaseUrl}/");

		await Expect(Page.Locator("#user")).ToHaveAttributeAsync("href", "/user/newcomer");
	}

	[Test]
	public async Task RegistrationRejectsAnEmailThatIsAlreadyTaken(){
		await OpenAsync("/login");

		await Page.GetByTestId("auth-mode-register").ClickAsync();
		await Page.GetByTestId("auth-identifier").FillAsync(TestData.AdminEmail);
		await Page.GetByTestId("auth-username").FillAsync("impostor");
		await Page.GetByTestId("auth-password").FillAsync(TestData.Password);
		await Page.GetByTestId("auth-submit").ClickAsync();

		await Expect(Page.GetByTestId("auth-error")).ToContainTextAsync("already taken");
		await Expect(Page).ToHaveURLAsync($"{BaseUrl}/login");
	}

	[Test]
	public async Task LoginSignsTheUserIn(){
		await LoginAsShopperAsync();

		await Expect(Page.Locator("#user"))
			.ToHaveAttributeAsync("href", $"/user/{TestData.ShopperUsername}");
	}

	[Test]
	public async Task LoginRejectsTheWrongPassword(){
		await OpenAsync("/login");

		await Page.GetByTestId("auth-identifier").FillAsync(TestData.ShopperUsername);
		await Page.GetByTestId("auth-password").FillAsync("NotMyPassword1");
		await Page.GetByTestId("auth-submit").ClickAsync();

		await Expect(Page.GetByTestId("auth-error")).ToHaveTextAsync("Incorrect password");
		await Expect(Page.Locator("#user")).ToHaveAttributeAsync("href", "/login");
	}

	[Test]
	public async Task LoginRejectsAnUnknownAccount(){
		await OpenAsync("/login");

		await Page.GetByTestId("auth-identifier").FillAsync("ghost");
		await Page.GetByTestId("auth-password").FillAsync(TestData.Password);
		await Page.GetByTestId("auth-submit").ClickAsync();

		await Expect(Page.GetByTestId("auth-error")).ToContainTextAsync("does not exist");
	}

	[Test]
	public async Task LogoutReturnsTheBrowserToAGuestSession(){
		await LoginAsShopperAsync();

		await OpenAsync($"/user/{TestData.ShopperUsername}");
		await Page.GetByTestId("profile-logout").ClickAsync();

		await Page.WaitForURLAsync($"{BaseUrl}/");
		await Expect(Page.Locator("#user")).ToHaveAttributeAsync("href", "/login");
	}

	[Test]
	public async Task ProfileOfAnotherUserRedirectsHome(){
		await LoginAsShopperAsync();

		await Page.GotoAsync($"/user/{TestData.AdminUsername}");

		await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");
	}

	[Test]
	public async Task AdminAreaRedirectsANonAdmin(){
		await LoginAsShopperAsync();

		await Page.GotoAsync("/admin");

		await Expect(Page).ToHaveURLAsync($"{BaseUrl}/");
		await Expect(Page.Locator("#admin")).ToHaveCountAsync(0);
	}

	[Test]
	public async Task AdminSeesTheDashboardLink(){
		await LoginAsAdminAsync();

		await Expect(Page.Locator("#admin")).ToBeVisibleAsync();

		await Page.Locator("#admin").ClickAsync();

		await Expect(Page.GetByTestId("admin-dashboard")).ToBeVisibleAsync();
	}
}
