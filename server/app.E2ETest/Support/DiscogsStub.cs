namespace app.E2ETest.Support;

public static class DiscogsStub{
	public const string NewBarcode = "9900000000001";
	public const string NewTitle = "Remain in Light";
	public const string NewArtist = "Talking Heads";

	public const string SecondBarcode = "9900000000002";
	public const string SecondTitle = "Fear of Music";

	public static Task InstallAsync(IPage page){
		return page.RouteAsync("**/Product/FetchProducts", route => route.FulfillAsync(new RouteFulfillOptions{
			Status = 200,
			ContentType = "application/json",
			Body = TwoMatchesJson()
		}));
	}

	private static string TwoMatchesJson(){
		return $$"""
		{
		  "data": [
		    {
		      "barcode": "{{NewBarcode}}",
		      "catalogNumber": "SRK-6095",
		      "name": "{{NewTitle}}",
		      "artist": "{{NewArtist}}",
		      "imageUrl": "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7",
		      "price": null,
		      "type": 0,
		      "runtime": "40:06",
		      "releaseDate": "1980",
		      "inStock": false,
		      "tracklist": [
		        {"title": "Born Under Punches", "runtime": "05:46"},
		        {"title": "Crosseyed and Painless", "runtime": "04:45"}
		      ]
		    },
		    {
		      "barcode": "{{SecondBarcode}}",
		      "catalogNumber": "SRK-6076",
		      "name": "{{SecondTitle}}",
		      "artist": "{{NewArtist}}",
		      "imageUrl": "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7",
		      "price": null,
		      "type": 0,
		      "runtime": "38:27",
		      "releaseDate": "1979",
		      "inStock": false,
		      "tracklist": [
		        {"title": "I Zimbra", "runtime": "03:07"}
		      ]
		    }
		  ]
		}
		""";
	}

	public static Task InstallFailureAsync(IPage page, string message){
		return page.RouteAsync("**/Product/FetchProducts", route => route.FulfillAsync(new RouteFulfillOptions{
			Status = 400,
			ContentType = "application/json",
			Body = $$"""{"message": "{{message}}"}"""
		}));
	}
}
