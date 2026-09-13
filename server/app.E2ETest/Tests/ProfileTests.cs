using app.E2ETest.Support;

namespace app.E2ETest.Tests;

/* Everything here runs against the throwaway account, because a successful
 * email or password change bumps the user's TokenVersion and signs them out.
 */
[TestFixture]
public class ProfileTests: VinyliumPageTest{
	[SetUp]
	public Task ResetBeforeEachTest() => Seeder.ResetAsync();

	private Task LoginAsThrowawayAsync() => LoginAsync(TestData.ThrowawayUsername);

	private async Task OpenEditMenuAsync(){
		await OpenAsync($"/user/{TestData.ThrowawayUsername}");
		await Page.GetByTestId("profile-edit-toggle").ClickAsync();
	}

	[Test]
	public async Task ProfileShowsNoOrdersForANewAccount(){
		await LoginAsThrowawayAsync();

		await OpenAsync($"/user/{TestData.ThrowawayUsername}");

		await Expect(Page.GetByTestId("profile-no-orders")).ToHaveTextAsync("No orders yet.");
	}

	[Test]
	public async Task TheEditMenuOffersBothChanges(){
		await LoginAsThrowawayAsync();
		await OpenEditMenuAsync();

		await Expect(Page.GetByTestId("profile-change-email")).ToBeVisibleAsync();
		await Expect(Page.GetByTestId("profile-change-password")).ToBeVisibleAsync();
		await Expect(Page.GetByTestId("profile-edit-toggle")).ToHaveTextAsync("Cancel");
	}

	[Test]
	public async Task ChangingTheEmailSignsTheUserOut(){
		await LoginAsThrowawayAsync();
		await OpenEditMenuAsync();

		await Page.GetByTestId("profile-change-email").ClickAsync();
		await Page.GetByTestId("profile-new-email").FillAsync("renamed@vinylium.test");
		await Page.GetByTestId("profile-current-password").FillAsync(TestData.Password);
		await Page.GetByTestId("profile-save").ClickAsync();

		/* the change bumps TokenVersion, so the logout call that follows it is
		 * rejected, the refresh interceptor gives up and hard-navigates here
		 */
		await Page.WaitForURLAsync($"{BaseUrl}/login");
		await Expect(Page.Locator("#user")).ToHaveAttributeAsync("href", "/login");

		/* the new address is the one that works now */
		await LoginAsync("renamed@vinylium.test");
	}

	[Test]
	public async Task ChangingTheEmailRejectsAMalformedAddress(){
		await LoginAsThrowawayAsync();
		await OpenEditMenuAsync();

		await Page.GetByTestId("profile-change-email").ClickAsync();
		await Page.GetByTestId("profile-new-email").FillAsync("not-an-email");
		await Page.GetByTestId("profile-current-password").FillAsync(TestData.Password);
		await Page.GetByTestId("profile-save").ClickAsync();

		await Expect(Page.GetByTestId("profile-error"))
			.ToHaveTextAsync("Email address format not valid");
	}

	[Test]
	public async Task ChangingTheEmailNeedsTheCurrentPassword(){
		await LoginAsThrowawayAsync();
		await OpenEditMenuAsync();

		await Page.GetByTestId("profile-change-email").ClickAsync();
		await Page.GetByTestId("profile-new-email").FillAsync("renamed@vinylium.test");
		await Page.GetByTestId("profile-save").ClickAsync();

		await Expect(Page.GetByTestId("profile-error"))
			.ToHaveTextAsync("Enter your current password to confirm");
	}

	[Test]
	public async Task ChangingTheEmailRejectsAnAddressAlreadyInUse(){
		await LoginAsThrowawayAsync();
		await OpenEditMenuAsync();

		await Page.GetByTestId("profile-change-email").ClickAsync();
		await Page.GetByTestId("profile-new-email").FillAsync(TestData.AdminEmail);
		await Page.GetByTestId("profile-current-password").FillAsync(TestData.Password);
		await Page.GetByTestId("profile-save").ClickAsync();

		await Expect(Page.GetByTestId("profile-error")).ToContainTextAsync("already taken");
	}

	[Test]
	public async Task ChangingThePasswordSignsTheUserOut(){
		await LoginAsThrowawayAsync();
		await OpenEditMenuAsync();

		await Page.GetByTestId("profile-change-password").ClickAsync();
		await Page.GetByTestId("profile-current-password").FillAsync(TestData.Password);
		await Page.GetByTestId("profile-new-password").FillAsync("BrandNewPass1!");
		await Page.GetByTestId("profile-save").ClickAsync();

		await Page.WaitForURLAsync($"{BaseUrl}/login");
		await Expect(Page.Locator("#user")).ToHaveAttributeAsync("href", "/login");

		/* the old password is gone and the new one works */
		await LoginAsync(TestData.ThrowawayUsername, "BrandNewPass1!");
	}

	[Test]
	public async Task ChangingThePasswordRejectsAShortOne(){
		await LoginAsThrowawayAsync();
		await OpenEditMenuAsync();

		await Page.GetByTestId("profile-change-password").ClickAsync();
		await Page.GetByTestId("profile-current-password").FillAsync(TestData.Password);
		await Page.GetByTestId("profile-new-password").FillAsync("short");
		await Page.GetByTestId("profile-save").ClickAsync();

		await Expect(Page.GetByTestId("profile-error"))
			.ToHaveTextAsync("Password must be at least 8 characters");
	}

	[Test]
	public async Task ChangingThePasswordRejectsTheWrongCurrentPassword(){
		await LoginAsThrowawayAsync();
		await OpenEditMenuAsync();

		await Page.GetByTestId("profile-change-password").ClickAsync();
		await Page.GetByTestId("profile-current-password").FillAsync("NotMyPassword1");
		await Page.GetByTestId("profile-new-password").FillAsync("BrandNewPass1!");
		await Page.GetByTestId("profile-save").ClickAsync();

		await Expect(Page.GetByTestId("profile-error")).ToHaveTextAsync("Incorrect password");
	}
}
