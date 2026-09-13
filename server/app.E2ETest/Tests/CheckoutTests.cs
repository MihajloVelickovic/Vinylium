using app.E2ETest.Support;

namespace app.E2ETest.Tests;

[TestFixture]
public class CheckoutTests: VinyliumPageTest{
	/* every test here consumes stock and creates orders, so each one starts
	 * from the seeded baseline instead of from whatever the previous left
	 */
	[SetUp]
	public Task ResetBeforeEachTest() => Seeder.ResetAsync();

	[Test]
	public async Task GuestCanCheckOutWithAnEmailAddress(){
		await AddToCartAsync(TestData.Nevermind);
		await OpenAsync("/checkout");

		await Page.GetByTestId("checkout-email").FillAsync("guest@vinylium.test");
		await Page.GetByTestId("checkout-finish").ClickAsync();

		await Expect(Page.GetByTestId("checkout-confirmation")).ToBeVisibleAsync();
		await Expect(Page.GetByTestId("checkout-confirmation"))
			.ToContainTextAsync("guest@vinylium.test");
		await Expect(Page.GetByTestId("checkout-order-id")).ToContainTextAsync("Order #");
		await Expect(Page.GetByTestId("checkout-total")).ToHaveTextAsync("Total: 900 RSD");
	}

	[Test]
	public async Task CheckoutEmptiesTheCart(){
		await AddToCartAsync(TestData.Nevermind);
		await OpenAsync("/checkout");

		await Page.GetByTestId("checkout-email").FillAsync("guest@vinylium.test");
		await Page.GetByTestId("checkout-finish").ClickAsync();
		await Expect(Page.GetByTestId("checkout-confirmation")).ToBeVisibleAsync();

		await OpenAsync("/cart");

		await Expect(Page.GetByTestId("cart-empty")).ToBeVisibleAsync();
	}

	[Test]
	public async Task CheckoutTakesTheCopiesOutOfTheStoresStock(){
		await OpenAsync($"/products/{TestData.Nevermind}");
		await Expect(Page.GetByTestId("product-store-select"))
			.ToContainTextAsync($"{TestData.CentarStore} (3 in stock)");

		await ClickAddToCartAsync();
		await OpenAsync("/checkout");
		await Page.GetByTestId("checkout-email").FillAsync("guest@vinylium.test");
		await Page.GetByTestId("checkout-finish").ClickAsync();
		await Expect(Page.GetByTestId("checkout-confirmation")).ToBeVisibleAsync();

		await OpenAsync($"/products/{TestData.Nevermind}");

		await Expect(Page.GetByTestId("product-store-select"))
			.ToContainTextAsync($"{TestData.CentarStore} (2 in stock)");
	}

	[Test]
	public async Task CheckoutCannotBeSubmittedWithoutAnEmailAddress(){
		await AddToCartAsync(TestData.Nevermind);
		await OpenAsync("/checkout");

		await Expect(Page.GetByTestId("checkout-finish")).ToBeDisabledAsync();
	}

	[Test]
	public async Task CheckoutRejectsAMalformedEmailAddress(){
		await AddToCartAsync(TestData.Nevermind);
		await OpenAsync("/checkout");

		await Page.GetByTestId("checkout-email").FillAsync("not-an-email");
		await Page.GetByTestId("checkout-finish").ClickAsync();

		await Expect(Page.GetByTestId("checkout-error"))
			.ToHaveTextAsync("Email address format not valid");
	}

	[Test]
	public async Task AnEmptyCartCannotBeCheckedOut(){
		await OpenAsync("/checkout");

		await Expect(Page.GetByTestId("checkout-empty")).ToHaveTextAsync("Your cart is empty.");
		await Expect(Page.GetByTestId("checkout-finish")).ToHaveCountAsync(0);
	}

	[Test]
	public async Task SignedInCheckoutPrefillsTheAccountEmail(){
		await LoginAsShopperAsync();
		await AddToCartAsync(TestData.Nevermind);

		await OpenAsync("/checkout");

		await Expect(Page.GetByTestId("checkout-email")).ToHaveValueAsync(TestData.ShopperEmail);
	}

	[Test]
	public async Task AnOrderPlacedWhileSignedInShowsUpOnTheProfile(){
		await LoginAsShopperAsync();
		await AddToCartAsync(TestData.Nevermind);

		await OpenAsync("/checkout");
		await Page.GetByTestId("checkout-finish").ClickAsync();
		await Expect(Page.GetByTestId("checkout-confirmation")).ToBeVisibleAsync();

		await OpenAsync($"/user/{TestData.ShopperUsername}");

		await Expect(Page.GetByTestId("order-summary")).ToHaveCountAsync(1);
		await Expect(Page.GetByTestId("profile-orders")).ToContainTextAsync("900 RSD");
	}

	[Test]
	public async Task AFreshOrderCanBeCancelled(){
		await LoginAsShopperAsync();
		await AddToCartAsync(TestData.Nevermind);

		await OpenAsync("/checkout");
		await Page.GetByTestId("checkout-finish").ClickAsync();
		await Expect(Page.GetByTestId("checkout-confirmation")).ToBeVisibleAsync();

		await OpenAsync($"/user/{TestData.ShopperUsername}");
		await Page.GetByTestId("order-cancel").ClickAsync();

		await ExpectToastAsync("Order cancelled");
		await Expect(Page.GetByTestId("profile-no-orders")).ToBeVisibleAsync();
	}
}
