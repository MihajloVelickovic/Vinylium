using System.Globalization;
using app.Enums;
using app.Models;
using Newtonsoft.Json.Linq;

namespace app.Helper;

public static class Discogs{
	
	private static readonly HttpClient _client =  new();
	private static string? _key = null!;
	private static string? _secret = null!;

	public static void Authorize(string? key, string? secret){
		_key = key;
		_secret = secret;
	}
	
	public static async Task<Product> AlbumData(string barcode){
		_client.DefaultRequestHeaders.Add("User-Agent", "VinyliumApp/1.0");
		
		var master = await GetMaster(barcode);
		var response = await GetMetadata(master.Item1);
		
		return new Product(){
			Name = (string)response["title"]!,
			Artist = (string)response["artists"]![0]!["name"]!,
			ImageUrl = (string)response["images"]![0]!["resource_url"]!,
			Price = 1000,
			Runtime = GetRuntime(response),
			Type = ProductType.Cd,
			ReleaseDate = (string)response["year"]!,
			InWarehouse = false,
			Tracklist = GetTracklist(response),
		};
	}

	private static List<string> GetTracklist(JObject o){
		var tracklist = o["tracklist"]!;
		return tracklist.Select(track => (string)track!["title"]!).ToList();
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
	
	private static async Task<JObject> GetMetadata(string master){
		var requestString = MasterString(master);
		using var response = await _client.GetAsync(requestString);
		if(!response.IsSuccessStatusCode)
			throw new Exception(response.ReasonPhrase);

		var responseString =  await response.Content.ReadAsStringAsync();
		var responseJObject = JObject.Parse(responseString);
		return responseJObject;
	}

	private static async Task<Tuple<string,string>> GetMaster(string barcode){
		var prefix = SearchPrefix();
		var pagination = Pagination(1, 1);
		var suffix = AuthSuffix();
		var requestString = $"{prefix}barcode={barcode}{pagination}{suffix}";
		
		using var response = await _client.GetAsync(requestString);
		if(!response.IsSuccessStatusCode)
			throw new Exception(response.ReasonPhrase);

		var responseString =  await response.Content.ReadAsStringAsync();
		var responseJObject = JObject.Parse(responseString);
		var dataField = responseJObject["results"]![0]!;
		var master = (string)dataField["master_id"]!;
		var format = (string)dataField["format"]![0]!;
		return new(master, format);
	}
	
	private static string AuthSuffix(){
		return _key != null && _secret != null ? $"&key={_key}&secret={_secret}" : string.Empty;
	}

	private static string Pagination(int pages, int perPage){
		return $"&pages={pages}&per_page={perPage}";
		
	}

	private static string MasterString(string master){
		return $"https://api.discogs.com/masters/{master}";
	}
	
	private static string SearchPrefix(){
		return "https://api.discogs.com/database/search?";
	}
	
}