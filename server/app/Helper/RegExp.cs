using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace app.Helper;

public static class RegExp{

	public static bool Check(string pattern, string input){
		
		var regex = new Regex(pattern, 
			                  RegexOptions.None,
							  TimeSpan.FromSeconds(1));
		
		return regex.IsMatch(input);
		
	}
}

