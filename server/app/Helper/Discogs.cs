using System.Globalization;
using System.Net.Http.Headers;
using app.Enums;
using app.Models;
using Newtonsoft.Json.Linq;

namespace app.Helper;

public static class Discogs{
	
	private static readonly HttpClient Client =  new(){
		DefaultRequestHeaders ={
			/* future proofing to always hit the v2 api endpoints, per documentation*/
			Accept = { new ("application/vnd.discogs.v2.discogs+json") }, 
			/* discogs api requires a User-Agent */
			UserAgent = { new ("VinyliumApp","1.0") }
		},
	};
	private static string? _key = null!;
	private static string? _secret = null!;

	public static void Authorize(string? key, string? secret){
		_key = key;
		_secret = secret;
	}

	public static async Task<List<Product>> CreateProduct(string code, bool isBarcode, decimal price){
		
		/* entry data will be JArray if isBarcode 
		 * is false and JToken if it's false.
		 * responseJson is a helper to deal 
		 * with these different scenarios
		 */
		var entryData = await GetEntryData(code, isBarcode) ?? throw new Exception("Couldn't parse entry data");
		var responseJson = entryData;
		if(isBarcode){
			responseJson = new JArray(entryData);
		}
		
		/* list that will be returned
		 * the logic behind this is that barcodes are unique,
		 * while catalog numbers aren't, so if a catalog number
		 * is inputted, we want to return every release that
		 * number points to and let the user choose
		 */
		var list = new List<Product>();

		foreach(var data in responseJson){
			
			var formatString = (string?)data["format"]?[0] ?? "";
			var releaseId = (string?)data["id"];
			var releaseData = await GetReleaseData(releaseId);

			/* deals with different barcode scenarios 
			 * that can happen when fetching data 
			 */
			var barcodeList = data["barcode"] ?? new JArray();
			
			var barcode = barcodeList.Any() ? 
						 (string?)barcodeList[0] ?? Guid.NewGuid().ToString() : 
						 Guid.NewGuid().ToString();
			
			list.Add(new Product(){
				Barcode = isBarcode ? code : barcode,
				CatalogNumber = !isBarcode ? code : ((string?)data["catno"]) ?? "",
				Name = (string?)releaseData["title"] ?? "",
				Artist = (string?)(releaseData["artists"]?[0]?["name"]) ?? "",
				ImageUrl = (string?)(releaseData["images"]?[0]?["resource_url"]) ?? "",
				Price = price,
				Runtime = GetRuntime(releaseData),
				Type = GetFormat(formatString),
				ReleaseDate = (string?)data["year"] ?? "",
				InWarehouse = false,
				Tracklist = GetTracklist(releaseData),
			});
		}
		return list;
	}

	private static async Task<JToken?> GetEntryData(string code, bool isBarcode){
		var prefix = SearchPrefix();
		var suffix = AuthSuffix();
		var searchParameter = isBarcode ? "barcode" : "catno";
		var requestString = $"{prefix}{searchParameter}={code}{suffix}";

		var response = await Client.GetAsync(requestString);
		
		var responseString = await response.Content.ReadAsStringAsync();
		var responseJObject = JObject.Parse(responseString);
		
		/* check if anything returned */
		var items = (int?)responseJObject["pagination"]?["items"] ?? throw new Exception("Couldn't parse pagination");
		if(items == 0)
			throw new Exception($"No items with code {code} found");
		
		var release = "0";
		var year = "0";
		var formatString = string.Empty;
		var index = -1;
		while(isBarcode && (release == "0" || release == null || 
		       formatString == string.Empty || formatString == null ||
		       year == "0" || year == null) &&
		      index < items - 1){
			
			++index;
			var dataField = responseJObject["results"]?[index] ?? throw new Exception("Couldn't parse data");
			release = (string?)dataField["id"];
			year = (string?)dataField["year"];
			formatString = (string?)dataField["format"]![0];
			
		}

		/* barcode searches can also result in multiple results
		 * but all of those are always the same album, just sometimes
		 * have some different metadata.
		 * If we're searching using barcodes, we want to return one
		 * specific release (the one with the best metadata as found
		 * above), but when searching with cat.no-s we want to return
		 * every release
		 */
		return isBarcode ? responseJObject["results"]?[index]  ?? throw new Exception("Couldn't parse data") : 
			               responseJObject["results"] ?? throw new Exception("Couldn't parse data");

	}
	
	private static async Task<JObject> GetReleaseData(string? release){
		if(release == null)
			throw new Exception("Release ID is null");
		var requestString = ReleaseString(release);
		using var response = await Client.GetAsync(requestString);
		if(!response.IsSuccessStatusCode)
			throw new Exception(response.ReasonPhrase);

		var responseString =  await response.Content.ReadAsStringAsync();
		var responseJObject = JObject.Parse(responseString);
		return responseJObject;
	}
	
	private static ProductType GetFormat(string formatString){
		return formatString.ToUpper().Contains("CD") ? ProductType.Cd :
			   formatString.ToUpper().Contains("VINYL") ||
			   formatString.ToUpper().Contains("LP") ||
			   formatString.ToUpper().Contains("EP") ? ProductType.Vinyl :
			   formatString.ToUpper().Contains("CASSETTE") ? ProductType.Cassette :
			   throw new Exception($"Product type ${formatString} not found");
	}
	
	private static string GetRuntime(JObject o){
		var tracklist = o["tracklist"] ?? null;
		if (tracklist == null)
			throw new Exception("No tracklist found");
		
		var runtime = TimeSpan.Zero;
		foreach(var track in tracklist){
			var type = (string?)track["type_"];
			switch(type){
				case null:
					throw new Exception("Track has no type_ field");
				case "track":{
					var durationString = (string?)track["duration"] ?? "";
					var splits = durationString.Split(':');
					if(splits[0].Length != 2)
						splits[0] = $"0{splits[0]}";
					durationString = string.Join(":", splits);
					var parsed = TimeSpan.TryParseExact(durationString, @"mm\:ss", CultureInfo.InvariantCulture,
						out var tempTime);
					if(parsed)
						runtime += tempTime;
					break;
				}
			}
		}
		return runtime.ToString();
	}
	
	private static List<string> GetTracklist(JObject o){
		var tracklist = o["tracklist"];
		if (tracklist == null)
			throw new Exception("No tracklist found");
		
		var list = new List<string>();
		foreach(var track in tracklist){
			var type = (string?)track["type_"];
			switch(type){
				case null:
					throw new Exception("Track has no type_ field");
				case "track":
					list.Add((string?)track["title"] ?? throw new Exception("Track has no title field"));
					break;
			}
		}

		return list;
	}
	
	private static string AuthSuffix(){
		return _key != null && _secret != null ? $"&key={_key}&secret={_secret}" : string.Empty;
	}
	
	private static string ReleaseString(string release){
		return $"https://api.discogs.com/releases/{release}";
	}
	
	private static string SearchPrefix(){
		return "https://api.discogs.com/database/search?";
	}
	
}