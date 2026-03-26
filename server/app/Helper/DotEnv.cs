namespace app.Helper;

public static class DotEnv{
	public static void LoadFromFile(string path){
		try{
			foreach(var line in File.ReadLines(path)){
				var split = line.Split('=');
				if(split.Length != 2)
					return;
				split[0] = split[0].Trim();
				split[1] = split[1].Trim();

				Environment.SetEnvironmentVariable(split[0], split[1]);
			}
		}
		catch(Exception){
			return;
		}
	}

	public static string? Get(string var){
		return Environment.GetEnvironmentVariable(var);
	}
}