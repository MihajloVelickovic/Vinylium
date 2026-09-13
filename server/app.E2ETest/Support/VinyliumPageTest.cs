namespace app.E2ETest.Support;

public abstract class VinyliumPageTest: PageTest{
	protected const string BaseUrl = "http://localhost:5173";
	protected const string ApiUrl = "http://localhost:1738/api";

	private static bool RecordVideo =>
		Environment.GetEnvironmentVariable("VINYLIUM_VIDEO") == "1";

	public override BrowserNewContextOptions ContextOptions(){
		var options = new BrowserNewContextOptions{
			BaseURL = BaseUrl,
			ViewportSize = new ViewportSize{
				Width = 1280,
				Height = 720
			}
		};

		if(RecordVideo){
			options.RecordVideoDir = "../../../Videos";
			options.RecordVideoSize = new RecordVideoSize{
				Width = 1280,
				Height = 720
			};
		}

		return options;
	}
	
	[SetUp]
	public void AcceptDialogs(){
		Page.Dialog += AcceptDialog;
	}

	[TearDown]
	public void StopAcceptingDialogs(){
		Page.Dialog -= AcceptDialog;
	}

	private async void AcceptDialog(object? sender, IDialog dialog){
		try{
			await dialog.AcceptAsync();
		}
		catch(PlaywrightException){
			//ignored
		}
	}

	protected async Task OpenAsync(string path){
		await Page.GotoAsync(path);
		await Expect(Page.Locator("nav.navbar")).ToBeVisibleAsync();
	}

	protected async Task LoginAsync(string identifier, string password = TestData.Password){
		await OpenAsync("/login");

		await Page.GetByTestId("auth-identifier").FillAsync(identifier);
		await Page.GetByTestId("auth-password").FillAsync(password);
		await Page.GetByTestId("auth-submit").ClickAsync();

		await Expect(Page.GetByTestId("auth-message")).ToHaveTextAsync("Successful login");
		
		await Page.WaitForURLAsync($"{BaseUrl}/");
	}

	protected Task LoginAsAdminAsync() => LoginAsync(TestData.AdminUsername);

	protected Task LoginAsShopperAsync() => LoginAsync(TestData.ShopperUsername);

	protected ILocator Toast(string text){
		return Page.GetByTestId("toast").Filter(new LocatorFilterOptions{HasText = text});
	}
	
	protected async Task ExpectToastAsync(string text){
		await Expect(Toast(text)).ToBeVisibleAsync();
	}

	protected async Task AddToCartAsync(string barcode){
		await OpenAsync($"/products/{barcode}");
		await ClickAddToCartAsync();
	}

	protected async Task ClickAddToCartAsync(){
		await Page.RunAndWaitForResponseAsync(
			async () => await Page.GetByTestId("product-add-to-cart").ClickAsync(),
			response => response.Url.Contains("/Cart/AddItem"));
	}

	protected Task ScreenshotAsync(string name){
		return Page.ScreenshotAsync(new PageScreenshotOptions{
			Path = $"../../../Screenshots/{name}.png"
		});
	}
}
