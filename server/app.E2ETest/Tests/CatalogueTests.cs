using app.E2ETest.Support;

namespace app.E2ETest.Tests;

[TestFixture]
public class CatalogueTests: VinyliumPageTest{
	[Test]
	public async Task StoreListsEverySeededProduct(){
		await OpenAsync("/");

		await Expect(Page.GetByTestId("product-card")).ToHaveCountAsync(5);
		await Expect(Page.GetByTestId("product-grid"))
			.ToContainTextAsync(TestData.KindOfBlueTitle);
	}

	[Test]
	public async Task SearchNarrowsTheCatalogue(){
		await OpenAsync("/");
		await Expect(Page.GetByTestId("product-card")).ToHaveCountAsync(5);

		await Page.GetByTestId("filters-search").FillAsync("Nirvana");

		await Expect(Page.GetByTestId("product-card")).ToHaveCountAsync(1);
		await Expect(Page.GetByTestId("product-card"))
			.ToContainTextAsync(TestData.NevermindTitle);
	}

	[Test]
	public async Task SearchWithNoMatchesEmptiesTheGrid(){
		await OpenAsync("/");

		await Page.GetByTestId("filters-search").FillAsync("nothing matches this");

		await Expect(Page.GetByTestId("product-card")).ToHaveCountAsync(0);
	}

	[Test]
	public async Task TypeFilterKeepsOnlyTheChosenFormat(){
		await OpenAsync("/");

		await Page.GetByTestId("filter-type").SelectOptionAsync(new SelectOptionValue{Label = "Cassette"});

		await Expect(Page.GetByTestId("product-card")).ToHaveCountAsync(1);
		await Expect(Page.GetByTestId("product-card")).ToContainTextAsync(TestData.NevermindTitle);
	}

	[Test]
	public async Task PriceRangeExcludesProductsOutsideIt(){
		await OpenAsync("/");

		await Page.GetByTestId("filter-price-low").FillAsync("1000");
		await Page.GetByTestId("filter-price-high").FillAsync("2600");

		await Expect(Page.GetByTestId("product-card")).ToHaveCountAsync(2);
		await Expect(Page.GetByTestId("product-grid")).ToContainTextAsync(TestData.KindOfBlueTitle);
		await Expect(Page.GetByTestId("product-grid")).ToContainTextAsync(TestData.OkComputerTitle);
	}

	[Test]
	public async Task SlashShortcutFocusesTheSearchBox(){
		await OpenAsync("/");
		await Expect(Page.GetByTestId("product-card")).ToHaveCountAsync(5);

		await Page.Keyboard.PressAsync("/");

		await Expect(Page.GetByTestId("filters-search")).ToBeFocusedAsync();
	}

	[Test]
	public async Task EscapeLeavesTheSearchBox(){
		await OpenAsync("/");
		await Expect(Page.GetByTestId("product-card")).ToHaveCountAsync(5);

		await Page.GetByTestId("filters-search").ClickAsync();
		await Expect(Page.GetByTestId("filters-search")).ToBeFocusedAsync();

		await Page.Keyboard.PressAsync("Escape");

		await Expect(Page.GetByTestId("filters-search")).Not.ToBeFocusedAsync();
	}

	[Test]
	public async Task ProductPageShowsTheFullRelease(){
		await OpenAsync($"/products/{TestData.KindOfBlue}");

		await Expect(Page.GetByTestId("product-title")).ToContainTextAsync("Miles Davis");
		await Expect(Page.GetByTestId("product-title")).ToContainTextAsync("Kind of Blue");
		await Expect(Page.GetByTestId("product-price")).ToHaveTextAsync("2500 RSD");
		await Expect(Page.Locator("#type")).ToHaveTextAsync("Vinyl");
		await Expect(Page.Locator(".tracklistDetail")).ToHaveCountAsync(3);
	}

	[Test]
	public async Task ProductPagePreselectsTheFirstStoreInStock(){
		await OpenAsync($"/products/{TestData.OkComputer}");

		await Expect(Page.GetByTestId("product-store-select"))
			.ToHaveValueAsync("11111111-1111-4111-8111-111111111111");
		await Expect(Page.GetByTestId("product-add-to-cart")).ToBeEnabledAsync();
	}

	[Test]
	public async Task OutOfStockProductCannotBeAddedToTheCart(){
		await OpenAsync($"/products/{TestData.UnknownPleasures}");

		await Expect(Page.GetByTestId("product-add-to-cart")).ToHaveTextAsync("Out of Stock");
		await Expect(Page.GetByTestId("product-add-to-cart")).ToBeDisabledAsync();
	}
}
