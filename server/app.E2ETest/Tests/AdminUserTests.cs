using app.E2ETest.Support;

namespace app.E2ETest.Tests;

[TestFixture]
public class AdminUserTests: VinyliumPageTest{
	[SetUp]
	public Task ResetBeforeEachTest() => Seeder.ResetAsync();

	private ILocator UserCard(string username){
		return Page.GetByTestId("user-card")
		           .Filter(new LocatorFilterOptions{
			           Has = Page.GetByTestId("user-username")
			                     .Filter(new LocatorFilterOptions{HasTextString = username})
		           });
	}

	private async Task OpenManageUsersAsync(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/manage-users");
		await Expect(Page.GetByTestId("manage-user-grid")).ToBeVisibleAsync();
	}

	[Test]
	public async Task ManageUsersListsEverySeededAccount(){
		await OpenManageUsersAsync();

		await Expect(Page.GetByTestId("user-card")).ToHaveCountAsync(3);
		await Expect(UserCard(TestData.ShopperUsername)).ToBeVisibleAsync();
	}

	[Test]
	public async Task UsersCanBeFilteredByRole(){
		await OpenManageUsersAsync();

		await Page.GetByTestId("filter-role").SelectOptionAsync(new SelectOptionValue{Label = "Admin"});

		await Expect(Page.GetByTestId("user-card")).ToHaveCountAsync(1);
		await Expect(UserCard(TestData.AdminUsername)).ToBeVisibleAsync();
	}

	[Test]
	public async Task UsersCanBeSearched(){
		await OpenManageUsersAsync();

		await Page.GetByTestId("filters-search").FillAsync("shopper");

		await Expect(Page.GetByTestId("user-card")).ToHaveCountAsync(1);
		await Expect(UserCard(TestData.ShopperUsername)).ToBeVisibleAsync();
	}

	[Test]
	public async Task AUserCanBePromotedToAdmin(){
		await OpenManageUsersAsync();

		var card = UserCard(TestData.ShopperUsername);
		await Expect(card.GetByTestId("user-admin-status")).ToHaveTextAsync("False");

		await card.GetByTestId("user-toggle-admin").ClickAsync();

		await ExpectToastAsync($"Promoted \"{TestData.ShopperUsername}\"");
		await Expect(card.GetByTestId("user-admin-status")).ToHaveTextAsync("True");
		await Expect(card.GetByTestId("user-toggle-admin")).ToHaveTextAsync("Revoke Admin");
	}

	[Test]
	public async Task APromotedUserGetsTheDashboardAfterSigningInAgain(){
		await OpenManageUsersAsync();
		await UserCard(TestData.ShopperUsername).GetByTestId("user-toggle-admin").ClickAsync();
		await ExpectToastAsync($"Promoted \"{TestData.ShopperUsername}\"");

		await LoginAsShopperAsync();

		await Expect(Page.Locator("#admin")).ToBeVisibleAsync();
	}

	[Test]
	public async Task APromotedUserCanBeDemotedAgain(){
		await OpenManageUsersAsync();

		var card = UserCard(TestData.ShopperUsername);
		await card.GetByTestId("user-toggle-admin").ClickAsync();
		await Expect(card.GetByTestId("user-admin-status")).ToHaveTextAsync("True");

		await card.GetByTestId("user-toggle-admin").ClickAsync();

		await ExpectToastAsync($"Demoted \"{TestData.ShopperUsername}\"");
		await Expect(card.GetByTestId("user-admin-status")).ToHaveTextAsync("False");
	}

	[Test]
	public async Task AnAdminCannotRevokeTheirOwnStatus(){
		await OpenManageUsersAsync();

		var self = UserCard(TestData.AdminUsername).GetByTestId("user-toggle-admin");

		await Expect(self).ToHaveTextAsync("Revoke Admin");
		await Expect(self).ToBeDisabledAsync();
		await Expect(self).ToHaveAttributeAsync("title", "You cannot revoke your own admin status");
	}

	[Test]
	public async Task AUserCanBeDeleted(){
		await OpenManageUsersAsync();

		await UserCard(TestData.ThrowawayUsername).GetByTestId("user-delete").ClickAsync();

		await ExpectToastAsync($"Deleted \"{TestData.ThrowawayUsername}\"");
		await Expect(Page.GetByTestId("user-card")).ToHaveCountAsync(2);
		await Expect(UserCard(TestData.ThrowawayUsername)).ToHaveCountAsync(0);
	}

	[Test]
	public async Task ADeletedUserCanNoLongerSignIn(){
		await OpenManageUsersAsync();
		await UserCard(TestData.ThrowawayUsername).GetByTestId("user-delete").ClickAsync();
		await ExpectToastAsync($"Deleted \"{TestData.ThrowawayUsername}\"");

		await OpenAsync("/login");
		await Page.GetByTestId("auth-identifier").FillAsync(TestData.ThrowawayUsername);
		await Page.GetByTestId("auth-password").FillAsync(TestData.Password);
		await Page.GetByTestId("auth-submit").ClickAsync();

		await Expect(Page.GetByTestId("auth-error")).ToContainTextAsync("does not exist");
	}
}
