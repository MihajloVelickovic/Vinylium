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

	public static async Task<Product> CreateProduct(string code, decimal price){

		JToken entryData;
		try{
			entryData = await GetEntryData(code, true);
		}
		catch(Exception e){
			entryData = await GetEntryData(code, false);
		}
		
		var formatString = (string)entryData["format"]![0]!;
		var releaseId = (string)entryData["id"]!;
		var releaseData = await GetReleaseData(releaseId);
		
		return new Product(){
			Name = (string)entryData["title"]!,
			Artist = (string)releaseData["artists"]![0]!["name"]!,
			ImageUrl = (string)releaseData["images"]![0]!["resource_url"]!,
			Price = price,
			Runtime = GetRuntime(releaseData),
			Type = GetFormat(formatString),
			ReleaseDate = (string)entryData["year"]!,
			InWarehouse = false,
			Tracklist = GetTracklist(releaseData),
		};
		
	}

	private static async Task<JToken> GetEntryData(string code, bool isBarcode){
		var prefix = SearchPrefix();
		var suffix = AuthSuffix();
		var searchParameter = isBarcode ? "barcode" : "catno";
		var requestString = $"{prefix}{searchParameter}={code}{suffix}";

		var response = await Client.GetAsync(requestString);
		
		var responseString = await response.Content.ReadAsStringAsync();
		var responseJObject = JObject.Parse(responseString);
		
		/* check if anything returned */
		var items = (int)responseJObject["pagination"]!["items"]!;
		if(items == 0)
			throw new Exception($"No items with code {code} found");
		
		/* learned the hard way that some releases don't return proper
		 * master ids.....
		 * so we need to iterate through all releases until we find one
		 * also checking for the format just in case...
		 */
		var release = "0";
		var index = -1;
		var formatString = string.Empty;
		while((string.Equals(release, "0") ||
		       string.Equals(formatString, string.Empty)) &&
		      index < items - 1){
			
			++index;
			var dataField = responseJObject["results"]![index]!;
			release = (string)dataField["id"]!;
			formatString = (string)dataField["format"]![0]!;
			
		}
		return responseJObject["results"]![index]!;
		
	}
	
	private static async Task<JObject> GetReleaseData(string release){
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
		var tracklist = o["tracklist"]!;
		var runtime = TimeSpan.Zero;
		foreach(var track in tracklist){
			if((string)track["type_"]! == "track"){
				var durationString = (string)track!["duration"]!;
				var splits = durationString.Split(':');
				if(splits[0].Length != 2)
					splits[0] = $"0{splits[0]}";
				durationString = string.Join(":", splits);
				var parsed = TimeSpan.TryParseExact(durationString, @"mm\:ss", CultureInfo.InvariantCulture,
					out var tempTime);
				if(parsed)
					runtime += tempTime;
			}
		}
		return runtime.ToString();
	}
	
	private static List<string> GetTracklist(JObject o){
		var tracklist = o["tracklist"]!;
		var list = new List<string>();
		foreach(var track in tracklist){
			if((string)track["type_"]! == "track")	
				list.Add((string)track!["title"]!);
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