using app.E2ETest.Support;

namespace app.E2ETest.Tests;

[TestFixture]
public class CartMergeTests: VinyliumPageTest{
	[SetUp]
	public Task ResetBeforeEachTest() => Seeder.ResetAsync();

	private ILocator CartItem(string title){
		return Page.GetByTestId("cart-item")
		           .Filter(new LocatorFilterOptions{HasText = title});
	}

	private async Task LogoutAsync(){
		await OpenAsync($"/user/{TestData.ShopperUsername}");
		await Page.GetByTestId("profile-logout").ClickAsync();
		await Page.WaitForURLAsync($"{BaseUrl}/");
	}

	[Test]
	public async Task AGuestCartFollowsTheUserThroughLogin(){
		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");

		await LoginAsShopperAsync();

		await ExpectToastAsync("Merged 1 item from your guest cart");

		await OpenAsync("/cart");
		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(1);
		await Expect(CartItem(TestData.KindOfBlueTitle)).ToBeVisibleAsync();
	}

	[Test]
	public async Task SeveralGuestLinesAreAllCarriedOver(){
		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");
		await AddToCartAsync(TestData.Nevermind);
		await ExpectToastAsync("Added Nevermind to cart");

		await LoginAsShopperAsync();

		await ExpectToastAsync("Merged 2 items from your guest cart");

		await OpenAsync("/cart");
		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(2);
		await Expect(Page.GetByTestId("cart-total")).ToHaveTextAsync("Total: 3400 RSD");
	}

	[Test]
	public async Task QuantitiesAreSummedWhenBothCartsHoldTheSameRecord(){
		await LoginAsShopperAsync();
		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");
		await LogoutAsync();

		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");

		await LoginAsShopperAsync();
		await ExpectToastAsync("Merged 1 item from your guest cart");

		await OpenAsync("/cart");
		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(1);
		await Expect(Page.GetByTestId("cart-item-qty")).ToHaveValueAsync("2");
	}

	[Test]
	public async Task AMergeThatWouldExceedStockIsCappedAndReported(){
		await LoginAsShopperAsync();
		await AddToCartAsync(TestData.OkComputer);
		await ExpectToastAsync("Added OK Computer to cart");
		await LogoutAsync();

		await AddToCartAsync(TestData.OkComputer);
		await ExpectToastAsync("Added OK Computer to cart");

		await LoginAsShopperAsync();

		await ExpectToastAsync("limited to available stock: OK Computer");

		await OpenAsync("/cart");
		await Expect(Page.GetByTestId("cart-item-qty")).ToHaveValueAsync("1");
	}

	[Test]
	public async Task SigningInWithoutAGuestCartLeavesTheAccountCartAlone(){
		await LoginAsShopperAsync();
		await AddToCartAsync(TestData.DarkSideOfTheMoon);
		await ExpectToastAsync("Added The Dark Side of the Moon to cart");
		await LogoutAsync();

		await LoginAsShopperAsync();

		await OpenAsync("/cart");
		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(1);
		await Expect(CartItem(TestData.DarkSideTitle)).ToBeVisibleAsync();
	}

	[Test]
	public async Task AGuestCartIsNotVisibleToADifferentAccount(){
		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");

		await LoginAsShopperAsync();
		await ExpectToastAsync("Merged 1 item from your guest cart");
		await LogoutAsync();

		await LoginAsAdminAsync();

		await OpenAsync("/cart");
		await Expect(Page.GetByTestId("cart-empty")).ToBeVisibleAsync();
	}

	[Test]
	public async Task TheMergedCartCanBeCheckedOut(){
		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");

		await LoginAsShopperAsync();
		await ExpectToastAsync("Merged 1 item from your guest cart");

		await OpenAsync("/checkout");
		await Expect(Page.GetByTestId("checkout-email")).ToHaveValueAsync(TestData.ShopperEmail);

		await Page.GetByTestId("checkout-finish").ClickAsync();

		await Expect(Page.GetByTestId("checkout-confirmation")).ToBeVisibleAsync();
		await Expect(Page.GetByTestId("checkout-total")).ToHaveTextAsync("Total: 2500 RSD");
	}
}
