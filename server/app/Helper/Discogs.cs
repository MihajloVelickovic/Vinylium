using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using app.Enums;
using app.Models;
using Newtonsoft.Json.Linq;

namespace app.Helper;

public static class Discogs{
	private static readonly HttpClient Client = new(){
		DefaultRequestHeaders ={
			/* future proofing to always hit the v2 api endpoints, per documentation*/
			Accept = { new("application/vnd.discogs.v2.discogs+json") },
			/* discogs api requires a User-Agent */
			UserAgent = { new("VinyliumApp", "1.0") }
		},
	};

	private static string GenerateNumericCode(int length = 13){
		return Rng.Next().ToString($"D{length}");
	}
	
	private static string? _key;
	private static string? _secret;
	private static readonly Random Rng = new();

	public static void Authorize(string? key, string? secret){
		_key = key;
		_secret = secret;
	}

	public static async Task<List<Product>> CreateProduct(string code, decimal price){
		
		
		var productsBarcode = await GetEntryData(code, barcodeSearch: true) as JArray ?? [];
		var productsCatalogNumber = await GetEntryData(code, barcodeSearch: false) as JArray ?? [];
		
		if(!productsBarcode.Any() && !productsCatalogNumber.Any())
			throw new Exception($"No results found for {code}");

		/* will result in productsBarcode holding all available data no matter if any of the two arrays is empty */
		productsBarcode.Merge(productsCatalogNumber);
		
		var list = new List<Product>();
		var visited = new List<string>();
		
		foreach(var product in productsBarcode){
			
			/* for cases when the title itself contains a hyphen,
			 * joining is needed to keep it whole
			 */
			var titleSeparated = string.Join('-', ((string?)product["title"])?.Split('-').Skip(1) ?? []).Trim();
			
			/* avoiding rate limits and slow responses by only allowing one product with the same name
			 * to be added to the final list, useful for cases when the same barcode or catalog number 
			 * returns many same products with minimal differences (country, or some other minor detail)
			 * discogs api has a rate limit of 60 calls per minute for auth-ed users and 20 for
			 * non auth-ed so this is potentially a big improvement if it doesn't lead to faulty searches
			 */
			if(visited.Contains(titleSeparated, StringComparer.InvariantCultureIgnoreCase))
				continue;
			
			var formatString = (string?)product["format"]?[0] ?? "";
			
			/* release or master_release have better metadata than specific product queries
			 * such as: better images, they contain the tracklist, better formatting
			 */
			var releaseId = (string?)product["id"];
			var releaseData = await GetReleaseMasterData(releaseId, true);
			
			/* if the title from the request itself doesn't match the
			 * release title, try to do the same with the master release
			 */
			if(string.Compare((string?)releaseData["title"], titleSeparated, StringComparison.InvariantCultureIgnoreCase) != 0){
				var masterId = (string?)product["master_id"];
				if(masterId == null || masterId == "0")
					continue;
				releaseData = await GetReleaseMasterData(masterId, false);
				if((string?)releaseData["title"] != titleSeparated)
					continue;
			}
			
			/* If both release and master titles don't match the queried title, we cant consider
			 * it visited, so we only now add it to the list
			*/
			visited.Add(titleSeparated);
			
			/* deals with different barcode scenarios
			 * that can happen when fetching data
			 */
			var barcodeList = product["barcode"] ?? new JArray();
			string? barcode = null;
			if(barcodeList.Any()){
				foreach(var bc in barcodeList){
					var tempBc = (string?)bc;
					if(tempBc == null)
						continue;
					tempBc = tempBc.Replace(" ", "");
					barcode = tempBc;
					break;
				}
			}
			
			barcode ??= GenerateNumericCode();
			
			list.Add(new Product(){
				Barcode = barcode,
				CatalogNumber = ((string?)product["catno"]) ?? "",
				Name = (string?)releaseData["title"] ?? "",
				Artist = (string?)(releaseData["artists"]?[0]?["name"]) ?? "",
				ImageUrl = (string?)(releaseData["images"]?[0]?["resource_url"]) ?? "",
				Price = price,
				Runtime = GetRuntime(releaseData),
				Type = GetFormat(formatString),
				ReleaseDate = (string?)product["year"] ?? "",
				InWarehouse = false,
				Tracklist = GetTracklist(releaseData),
			});
		}
		return list.Count > 0 ? list : throw new Exception("No valid items found");
	}

	private static async Task<JToken?> GetEntryData(string code, bool barcodeSearch){
		
		var prefix = SearchPrefix();
		var suffix = AuthSuffix();
		var searchParameter = barcodeSearch ? "barcode" : "catno";
		var requestString = $"{prefix}{searchParameter}={code}{suffix}";

		var response = await Client.GetAsync(requestString);
		var responseString = await response.Content.ReadAsStringAsync();
		var responseJObject = JObject.Parse(responseString);

		/* check if anything returned */
		var items = (int?)responseJObject["pagination"]?["items"] ?? throw new Exception("Couldn't parse pagination");
		if(items == 0)
			return null;
		
		return responseJObject["results"] ?? throw new Exception("Couldn't parse data");
	}

	private static async Task<JObject> GetReleaseMasterData(string? id, bool release){
		if(id == null)
			throw new Exception("ID is null");
		var requestString = release ? ReleaseString(id) : MasterString(id);
		using var response = await Client.GetAsync(requestString);
		if(!response.IsSuccessStatusCode)
			throw new Exception(response.ReasonPhrase);

		var responseString = await response.Content.ReadAsStringAsync();
		var responseJObject = JObject.Parse(responseString);
		return responseJObject;
	}

	private static ProductType GetFormat(string formatString){
		return formatString.ToUpper().Contains("CD") ? ProductType.Cd :
			formatString.ToUpper().Contains("VINYL") ||
			formatString.ToUpper().Contains("LP") ||
			formatString.ToUpper().Contains("EP") ? ProductType.Vinyl :
			formatString.ToUpper().Contains("CASSETTE") ? ProductType.Cassette :
			ProductType.Unknown;
	}

	private static string GetRuntime(JObject o){
		var tracklist = o["tracklist"] ?? null;
		if(tracklist == null)
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
		if(tracklist == null)
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
	private static string MasterString(string master){
		return $"https://api.discogs.com/masters/{master}";
	}
	
	private static string SearchPrefix(){
		return "https://api.discogs.com/database/search?";
	}
	
}