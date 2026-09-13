using app.E2ETest.Support;

namespace app.E2ETest.Tests;

[TestFixture]
public class AdminProductTests: VinyliumPageTest{
	[SetUp]
	public Task ResetBeforeEachTest() => Seeder.ResetAsync();
	
	private ILocator ProductCard(string barcode){
		return Page.GetByTestId("manage-product-card")
		           .Filter(new LocatorFilterOptions{HasText = barcode});
	}

	private async Task OpenManageProductsAsync(){
		await LoginAsAdminAsync();
		await OpenAsync("/admin/manage-products");
		await Expect(Page.GetByTestId("manage-product-grid")).ToBeVisibleAsync();
	}

	private async Task FetchStubbedMatchesAsync(){
		await LoginAsAdminAsync();
		await DiscogsStub.InstallAsync(Page);

		await OpenAsync("/admin/add-album");
		await Page.GetByTestId("fetch-code").FillAsync(DiscogsStub.NewBarcode);
		await Page.GetByTestId("fetch-submit").ClickAsync();

		await Expect(Page.GetByTestId("fetch-match-heading")).ToHaveTextAsync("Top match");
	}

	[Test]
	public async Task ManageProductsListsEverySeededRelease(){
		await OpenManageProductsAsync();

		await Expect(Page.GetByTestId("manage-product-card")).ToHaveCountAsync(5);
		await Expect(ProductCard(TestData.KindOfBlue)).ToBeVisibleAsync();
	}

	[Test]
	public async Task ManageProductsCanBeFilteredByFormat(){
		await OpenManageProductsAsync();

		await Page.GetByTestId("filter-type").SelectOptionAsync(new SelectOptionValue{Label = "CD"});

		await Expect(Page.GetByTestId("manage-product-card")).ToHaveCountAsync(1);
		await Expect(ProductCard(TestData.OkComputer)).ToBeVisibleAsync();
	}

	[Test]
	public async Task AProductsPriceCanBeEdited(){
		await OpenManageProductsAsync();

		await ProductCard(TestData.KindOfBlue).ClickAsync();
		await Expect(Page.GetByTestId("edit-product-price")).ToHaveValueAsync("2500");

		await Page.GetByTestId("edit-product-price").FillAsync("2750");
		await Page.GetByTestId("edit-product-update").ClickAsync();

		await ExpectToastAsync("Updated \"Kind of Blue\"");

		await OpenAsync($"/products/{TestData.KindOfBlue}");
		await Expect(Page.GetByTestId("product-price")).ToHaveTextAsync("2750 RSD");
	}

	[Test]
	public async Task AProductCanBeDeleted(){
		await OpenManageProductsAsync();

		await ProductCard(TestData.UnknownPleasures).ClickAsync();
		await Page.GetByTestId("edit-product-delete").ClickAsync();

		await ExpectToastAsync("Deleted \"Unknown Pleasures\"");

		await OpenAsync("/");
		await Expect(Page.GetByTestId("product-card")).ToHaveCountAsync(4);
	}

	[Test]
	public async Task FetchingShowsTheTopMatchAndOffersTheRest(){
		await FetchStubbedMatchesAsync();

		await Expect(Page.GetByTestId("album-name")).ToHaveValueAsync(DiscogsStub.NewTitle);
		await Expect(Page.GetByTestId("album-artist")).ToHaveValueAsync(DiscogsStub.NewArtist);
		await Expect(Page.GetByTestId("fetch-see-others")).ToContainTextAsync("1 other match");
	}

	[Test]
	public async Task AnAlternateMatchCanBeBrowsedToAndPicked(){
		await FetchStubbedMatchesAsync();

		await Page.GetByTestId("fetch-see-others").ClickAsync();
		await Expect(Page.GetByTestId("fetch-crate")).ToBeVisibleAsync();

		await Page.Locator(".matchCard").First.ClickAsync();

		await Expect(Page.GetByTestId("fetch-match-heading")).ToHaveTextAsync("Match 2 of 2");
		await Expect(Page.GetByTestId("album-name")).ToHaveValueAsync(DiscogsStub.SecondTitle);
		await Expect(Page.GetByTestId("album-barcode")).ToHaveValueAsync(DiscogsStub.SecondBarcode);
	}

	[Test]
	public async Task AddingWithoutAPriceIsRefused(){
		await FetchStubbedMatchesAsync();

		await Expect(Page.GetByTestId("album-price-hint")).ToHaveTextAsync("Set a price before adding");

		await Page.GetByTestId("album-add").ClickAsync();

		await Expect(Page.GetByTestId("album-price-hint")).ToHaveTextAsync("Set a price before saving");
	}

	[Test]
	public async Task APriceWithTooManyDecimalsIsRefused(){
		await FetchStubbedMatchesAsync();

		await Page.GetByTestId("album-price").FillAsync("19.999");
		await Page.GetByTestId("album-add").ClickAsync();

		await Expect(Page.GetByTestId("album-price-hint"))
			.ToHaveTextAsync("Price cannot have more than 2 decimal places");
	}

	[Test]
	public async Task AFetchedReleaseCanBeAddedToTheCatalogue(){
		await FetchStubbedMatchesAsync();

		await Page.GetByTestId("album-price").FillAsync("2100");

		await Page.GetByTestId("album-store-quantity").First.FillAsync("4");

		await Page.GetByTestId("album-add").ClickAsync();

		await ExpectToastAsync($"Added \"{DiscogsStub.NewTitle}\" to the catalogue");
		await Expect(Page.GetByTestId("album-status"))
			.ToHaveTextAsync($"Added {DiscogsStub.NewTitle} to the catalogue");

		await OpenAsync($"/products/{DiscogsStub.NewBarcode}");
		await Expect(Page.GetByTestId("product-title")).ToContainTextAsync(DiscogsStub.NewArtist);
		await Expect(Page.GetByTestId("product-price")).ToHaveTextAsync("2100 RSD");
		await Expect(Page.GetByTestId("product-add-to-cart")).ToBeEnabledAsync();
	}

	[Test]
	public async Task AFailedLookupShowsTheServerMessage(){
		await LoginAsAdminAsync();
		await DiscogsStub.InstallFailureAsync(Page, "No results found for 0000000000000");

		await OpenAsync("/admin/add-album");
		await Page.GetByTestId("fetch-code").FillAsync("0000000000000");
		await Page.GetByTestId("fetch-submit").ClickAsync();

		await Expect(Page.GetByTestId("fetch-error"))
			.ToHaveTextAsync("No results found for 0000000000000");
	}
}
