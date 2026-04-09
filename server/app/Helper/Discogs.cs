using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
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

		var entryData = await GetEntryData(code, true);
		var entryDataCatalog = await GetEntryData(code, false) as JArray ?? [];
		if(entryData == null && entryDataCatalog == null)
			throw new Exception($"No results found for {code}");

		if(entryData != null)
			entryDataCatalog.AddFirst(entryData);
		
		var list = new List<Product>();
		
		foreach(var data in entryDataCatalog){
			var formatString = (string?)data["format"]?[0] ?? "";
			var releaseId = (string?)data["id"];
			var releaseData = await GetReleaseMasterData(releaseId, true);

			if((string?)releaseData["title"] != ((string?)data["title"])?.Split('-')[1].Trim()){
				var masterId = (string?)data["master_id"];
				if(masterId == null || masterId == "0")
					continue;
				releaseData = await GetReleaseMasterData(masterId, false);
			}
			
			/* deals with different barcode scenarios
			 * that can happen when fetching data
			 */
			var barcodeList = data["barcode"] ?? new JArray();

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
				CatalogNumber = ((string?)data["catno"]) ?? "",
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

		string? masterUrl = null;
		var release = "0";
		var year = "0";
		var formatString = string.Empty;
		var index = -1;
		while(barcodeSearch && (release == "0" || formatString == string.Empty || 
		                    formatString == string.Empty ||  year == "0" ||
		                    masterUrl == null) && index < items - 1){
			++index;
			var dataField = responseJObject["results"]?[index] ?? throw new Exception("Couldn't parse data");
			release = (string?)dataField["id"];
			year = (string?)dataField["year"];
			masterUrl = (string?)dataField["master_url"];
			formatString = (string?)dataField["format"]![0];
		}

		if(barcodeSearch && index > items - 1)
			throw new Exception("Couldn't find appropriate release");

		if(barcodeSearch && masterUrl == null)
			throw new Exception("Couldn't find master release");
		
		
		/* barcode searches can also result in multiple results
		 * but all of those are always the same album, just sometimes
		 * have some different metadata.
		 * If we're searching using barcodes, we want to return one
		 * specific release (the one with the best metadata as found
		 * above), but when searching with cat.no-s we want to return
		 * every releaserelease
		 */
		return barcodeSearch
			? responseJObject["results"]?[index] ?? throw new Exception("Couldn't parse data")
			: responseJObject["results"] ?? throw new Exception("Couldn't parse data");
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