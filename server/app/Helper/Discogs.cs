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
	
	public static async Task<Product> CreateProduct(string barcode, decimal price){
		
		var master = await GetMaster(barcode);
		var response = await GetMetadata(master.Item1);
		 
		return new Product(){
			Name = (string)response["title"]!,
			Artist = (string)response["artists"]![0]!["name"]!,
			ImageUrl = (string)response["images"]![0]!["resource_url"]!,
			Price = price,
			Runtime = GetRuntime(response),
			Type = GetFormat(master.Item2),
			ReleaseDate = (string)response["year"]!,
			InWarehouse = false,
			Tracklist = GetTracklist(response),
		};
		
	}
	
	private static async Task<Tuple<string,string>> GetMaster(string barcode){
		
		var prefix = SearchPrefix();
		var suffix = AuthSuffix();
		var requestString = $"{prefix}barcode={barcode}{suffix}";

		using var response = await Client.GetAsync(requestString);
		if(!response.IsSuccessStatusCode)
			throw new Exception(response.ReasonPhrase);

		var responseString = await response.Content.ReadAsStringAsync();
		var responseJObject = JObject.Parse(responseString);
		
		/* check if anything returned */
		var items = (int)responseJObject["pagination"]!["items"]!;
		if (items == 0)
			throw new Exception($"No items with barcode {barcode} found");
		
		/* learned the hard way that some releases don't return proper
		 * master ids.....
		 * so we need to iterate through all releases until we find one
		 * also checking for the format just in case...
		 */
		var master = "0";
		var index = 0;
		var format = string.Empty;
		while((string.Equals(master, "0") ||
		       string.Equals(format, string.Empty)) &&
		      index < items){
			var dataField = responseJObject["results"]![index]!;
			master = (string)dataField["master_id"]!;
			format = (string)dataField["format"]![0]!;
			++index;
		}
		
		return index <= items ? new (master, format) : throw new Exception("Couldn't find master and format");
		
	}
	
	private static async Task<JObject> GetMetadata(string master){
		var requestString = MasterString(master);
		using var response = await Client.GetAsync(requestString);
		if(!response.IsSuccessStatusCode)
			throw new Exception(response.ReasonPhrase);

		var responseString =  await response.Content.ReadAsStringAsync();
		var responseJObject = JObject.Parse(responseString);
		return responseJObject;
	}
	
	private static ProductType GetFormat(string formatString){
		return formatString.ToUpper() switch{
			"CD" => ProductType.Cd,
			"VINYL" => ProductType.Vinyl,
			"CASSETTE" => ProductType.Cassette,
			_ => throw new Exception($"Unknown format {formatString}")
		};
	}
	
	private static string GetRuntime(JObject o){
		var tracklist = o["tracklist"]!;
		var runtime = TimeSpan.Zero;
		foreach(var track in tracklist){
			var durationString = (string)track!["duration"]!;
			var splits = durationString.Split(':');
			if (splits[0].Length != 2)
				splits[0] = $"0{splits[0]}";
			durationString = string.Join(":", splits);
			var tempTime = TimeSpan.ParseExact(durationString, @"mm\:ss", CultureInfo.InvariantCulture);
			runtime += tempTime;
		}
		return runtime.ToString();
	}
	
	private static List<string> GetTracklist(JObject o){
		var tracklist = o["tracklist"]!;
		return tracklist.Select(track => (string)track!["title"]!).ToList();
	}
	
	private static string AuthSuffix(){
		return _key != null && _secret != null ? $"&key={_key}&secret={_secret}" : string.Empty;
	}
	
	private static string MasterString(string master){
		return $"https://api.discogs.com/masters/{master}";
	}
	
	private static string SearchPrefix(){
		return "https://api.discogs.com/database/search?";
	}
	
}