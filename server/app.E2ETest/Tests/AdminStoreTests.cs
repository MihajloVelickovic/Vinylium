using app.E2ETest.Support;

namespace app.E2ETest.Tests;

[TestFixture]
public class AdminStoreTests: VinyliumPageTest{
	[SetUp]
	public Task ResetBeforeEachTest() => Seeder.ResetAsync();

	private ILocator StoreCard(string name){
		return Page.GetByTestId("manage-store-card")
		           .Filter(new LocatorFilterOptions{HasText = name});
	}

	[Test]
	public async Task ManageStoresListsEverySeededStore(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/manage-stores");

		await Expect(Page.GetByTestId("manage-store-card")).ToHaveCountAsync(3);
		await Expect(StoreCard(TestData.WarehouseStore)).ToBeVisibleAsync();
	}

	[Test]
	public async Task StoresCanBeFilteredDownToWarehouses(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/manage-stores");

		await Page.GetByTestId("filter-warehouse").SelectOptionAsync(new SelectOptionValue{Label = "Yes"});

		await Expect(Page.GetByTestId("manage-store-card")).ToHaveCountAsync(1);
		await Expect(StoreCard(TestData.WarehouseStore)).ToBeVisibleAsync();
	}

	[Test]
	public async Task StoresCanBeSearchedByName(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/manage-stores");

		await Page.GetByTestId("filters-search").FillAsync("Duvaniste");

		await Expect(Page.GetByTestId("manage-store-card")).ToHaveCountAsync(1);
		await Expect(StoreCard(TestData.DuvanisteStore)).ToBeVisibleAsync();
	}

	[Test]
	public async Task AStoreCanBeCreated(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/add-store");

		await Page.GetByTestId("store-name").FillAsync("Vinylium Palilula");
		await Page.GetByTestId("store-address").FillAsync("Iglena 14");
		await Page.GetByTestId("store-city").FillAsync(TestData.PrimaryCity);
		await Page.GetByTestId("store-contact").FillAsync("+381601234570");
		await Page.GetByTestId("store-opening").FillAsync("09:30");
		await Page.GetByTestId("store-opening").BlurAsync();
		await Page.GetByTestId("store-closing").FillAsync("19:30");
		await Page.GetByTestId("store-closing").BlurAsync();

		await Page.GetByTestId("store-submit").ClickAsync();

		await Expect(Page.GetByTestId("store-message"))
			.ToHaveTextAsync("Added store \"Vinylium Palilula\"");

		await OpenAsync("/admin/manage-stores");
		await Expect(StoreCard("Vinylium Palilula")).ToBeVisibleAsync();
	}

	[Test]
	public async Task CreatingAStoreRejectsABadPhoneNumber(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/add-store");

		await Page.GetByTestId("store-name").FillAsync("Vinylium Medijana");
		await Page.GetByTestId("store-address").FillAsync("Zvucna 9");
		await Page.GetByTestId("store-city").FillAsync(TestData.PrimaryCity);
		await Page.GetByTestId("store-contact").FillAsync("12345");
		await Page.GetByTestId("store-opening").FillAsync("09:00");
		await Page.GetByTestId("store-opening").BlurAsync();
		await Page.GetByTestId("store-closing").FillAsync("20:00");
		await Page.GetByTestId("store-closing").BlurAsync();

		await Page.GetByTestId("store-submit").ClickAsync();

		await Expect(Page.GetByTestId("store-error"))
			.ToHaveTextAsync("Contact number not valid Serbian phone number");
	}

	[Test]
	public async Task CreatingAStoreRejectsADuplicateName(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/add-store");

		await Page.GetByTestId("store-name").FillAsync(TestData.CentarStore);
		await Page.GetByTestId("store-address").FillAsync("Bezimena 1");
		await Page.GetByTestId("store-city").FillAsync(TestData.PrimaryCity);
		await Page.GetByTestId("store-contact").FillAsync("+381601234571");
		await Page.GetByTestId("store-opening").FillAsync("09:00");
		await Page.GetByTestId("store-opening").BlurAsync();
		await Page.GetByTestId("store-closing").FillAsync("20:00");
		await Page.GetByTestId("store-closing").BlurAsync();

		await Page.GetByTestId("store-submit").ClickAsync();

		await Expect(Page.GetByTestId("store-error")).Not.ToBeEmptyAsync();
	}

	[Test]
	public async Task AnInvalidOpeningTimeIsRewritten(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/add-store");

		await Page.GetByTestId("store-opening").FillAsync("nonsense");
		await Page.GetByTestId("store-opening").BlurAsync();

		await Expect(Page.GetByTestId("store-opening")).ToHaveValueAsync("00:00");
	}

	[Test]
	public async Task AStoreCanBeEdited(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/manage-stores");

		await StoreCard(TestData.DuvanisteStore).ClickAsync();
		await Expect(Page.GetByTestId("edit-store-card")).ToBeVisibleAsync();

		await Page.GetByTestId("edit-store-city").FillAsync(TestData.WarehouseCity);
		await Page.GetByTestId("edit-store-update").ClickAsync();

		await ExpectToastAsync($"Updated \"{TestData.DuvanisteStore}\"");

		await OpenAsync("/admin/manage-stores");
		await Expect(StoreCard(TestData.DuvanisteStore)).ToContainTextAsync(TestData.WarehouseCity);
	}

	[Test]
	public async Task AStoreCanBeDeleted(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/manage-stores");

		await StoreCard(TestData.DuvanisteStore).ClickAsync();
		await Expect(Page.GetByTestId("edit-store-card")).ToBeVisibleAsync();

		await Page.GetByTestId("edit-store-delete").ClickAsync();

		await ExpectToastAsync($"Deleted \"{TestData.DuvanisteStore}\"");
		await Page.WaitForURLAsync($"{BaseUrl}/admin/manage-stores");

		await Expect(Page.GetByTestId("manage-store-card")).ToHaveCountAsync(2);
		await Expect(StoreCard(TestData.DuvanisteStore)).ToHaveCountAsync(0);
	}
}
