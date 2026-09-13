using app.E2ETest.Support;

namespace app.E2ETest.Tests;

[TestFixture]
public class CartTests: VinyliumPageTest{
	private ILocator CartItem(string title){
		return Page.GetByTestId("cart-item")
		           .Filter(new LocatorFilterOptions{HasText = title});
	}

	[Test]
	public async Task GuestCanAddAProductToTheCart(){
		await AddToCartAsync(TestData.KindOfBlue);

		await ExpectToastAsync("Added Kind of Blue to cart");

		await OpenAsync("/cart");

		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(1);
		await Expect(CartItem(TestData.KindOfBlueTitle)).ToBeVisibleAsync();
		await Expect(Page.GetByTestId("cart-total")).ToHaveTextAsync("Total: 2500 RSD");
	}

	[Test]
	public async Task AnEmptyCartSaysSo(){
		await OpenAsync("/cart");

		await Expect(Page.GetByTestId("cart-empty")).ToHaveTextAsync("Your cart is empty.");
		await Expect(Page.GetByTestId("cart-checkout")).ToHaveCountAsync(0);
	}

	[Test]
	public async Task AddingTheSameProductTwiceKeepsOneLine(){
		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");

		await ClickAddToCartAsync();

		await OpenAsync("/cart");

		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(1);
		await Expect(Page.GetByTestId("cart-item-qty")).ToHaveValueAsync("2");
		await Expect(Page.GetByTestId("cart-total")).ToHaveTextAsync("Total: 5000 RSD");
	}

	[Test]
	public async Task ChangingTheQuantityRecalculatesTheTotals(){
		await AddToCartAsync(TestData.DarkSideOfTheMoon);
		await OpenAsync("/cart");

		await Page.GetByTestId("cart-item-qty").FillAsync("3");

		await Expect(Page.GetByTestId("cart-item-subtotal")).ToHaveTextAsync("9600 RSD");
		await Expect(Page.GetByTestId("cart-total")).ToHaveTextAsync("Total: 9600 RSD");
	}

	[Test]
	public async Task TwoDifferentProductsSumIntoTheTotal(){
		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");

		await AddToCartAsync(TestData.Nevermind);
		await ExpectToastAsync("Added Nevermind to cart");

		await OpenAsync("/cart");

		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(2);
		await Expect(Page.GetByTestId("cart-total")).ToHaveTextAsync("Total: 3400 RSD");
	}

	[Test]
	public async Task RemovingTheLastItemEmptiesTheCart(){
		await AddToCartAsync(TestData.KindOfBlue);
		await OpenAsync("/cart");

		await Page.GetByTestId("cart-remove").ClickAsync();

		await ExpectToastAsync("Removed Kind of Blue from cart");
		await Expect(Page.GetByTestId("cart-empty")).ToBeVisibleAsync();
	}

	[Test]
	public async Task RemovingOneOfTwoLeavesTheOther(){
		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");
		await AddToCartAsync(TestData.Nevermind);
		await ExpectToastAsync("Added Nevermind to cart");

		await OpenAsync("/cart");
		await CartItem(TestData.KindOfBlueTitle).GetByTestId("cart-remove").ClickAsync();

		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(1);
		await Expect(CartItem(TestData.NevermindTitle)).ToBeVisibleAsync();
	}

	[Test]
	public async Task AskingForMoreThanTheStoreHasIsRefused(){
		await AddToCartAsync(TestData.OkComputer);
		await OpenAsync("/cart");

		await Page.GetByTestId("cart-item-qty").FillAsync("2");

		await ExpectToastAsync($"Only 1 of {TestData.OkComputer} available at the selected store");
		await Expect(Page.GetByTestId("cart-item-qty")).ToHaveValueAsync("1");
	}

	[Test]
	public async Task TheCartSurvivesAReload(){
		await AddToCartAsync(TestData.KindOfBlue);
		await ExpectToastAsync("Added Kind of Blue to cart");

		await OpenAsync("/cart");
		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(1);

		await Page.ReloadAsync();

		await Expect(Page.GetByTestId("cart-item")).ToHaveCountAsync(1);
		await Expect(CartItem(TestData.KindOfBlueTitle)).ToBeVisibleAsync();
	}

	[Test]
	public async Task CheckoutButtonLeadsToTheCheckoutPage(){
		await AddToCartAsync(TestData.KindOfBlue);
		await OpenAsync("/cart");

		await Page.GetByTestId("cart-checkout").ClickAsync();

		await Expect(Page).ToHaveURLAsync($"{BaseUrl}/checkout");
		await Expect(Page.GetByTestId("checkout-total")).ToHaveTextAsync("Total: 2500 RSD");
	}
}
