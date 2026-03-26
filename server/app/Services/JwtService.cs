using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using app.Helper;
using app.Repositories;

namespace app.Services;

public interface IJwtService{
	string GenerateAccessToken(string username, string email, bool admin);
	Task<string> GenerateRefreshToken(string username, int userId, string id);
	Task<ClaimsPrincipal?> ValidateToken(string token, bool refresh = false);
	Task<string?> GetUsernameFromToken(string token, bool refresh = false);
	public Task DeleteRefreshToken(string token);
}

public class JwtService: IJwtService{
	private readonly string _jwtSecret;
	private readonly string _refreshSecret;
	private readonly double _jwtExpMinutes;
	private readonly double _refreshExpMinutes;

	private readonly IJwtRepository _jwtRepository;

	public JwtService(IJwtRepository jwtRepo){
		_jwtSecret = DotEnv.Get("JWT_SECRET") ??
		             throw new InvalidOperationException("JWT_SECRET not found in environment");
		_refreshSecret = DotEnv.Get("JWT_REFRESH_SECRET") ??
		                 throw new InvalidOperationException("JWT_REFRESH_SECRET not found in environment");
		_jwtExpMinutes = double.Parse(DotEnv.Get("JWT_EXP") ?? "10");
		_refreshExpMinutes = double.Parse(DotEnv.Get("JWT_REFRESH_EXP") ?? "25200");
		_jwtRepository = jwtRepo;
	}

	public string GenerateAccessToken(string username, string email, bool admin){
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtSecret);

		var claims = new List<Claim>{
			new(ClaimTypes.Name, username),
			new(ClaimTypes.Email, email),
			new("username", username),
			new("email", email),
		};

		if(admin)
			claims.Add(new Claim(ClaimTypes.Role, "Admin"));

		var tokenDescriptor = new SecurityTokenDescriptor{
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.UtcNow.AddMinutes(_jwtExpMinutes),
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature)
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}

	public async Task<string> GenerateRefreshToken(string username, int userId, string id){
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_refreshSecret);

		var tokenDescriptor = new SecurityTokenDescriptor{
			Subject = new ClaimsIdentity([
				new Claim(ClaimTypes.Name, username),
				new Claim("username", username),
				new Claim(JwtRegisteredClaimNames.Jti, id)
			]),
			Expires = DateTime.UtcNow.AddMinutes(_refreshExpMinutes),
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature)
		};
		var token = tokenHandler.CreateToken(tokenDescriptor);
		await _jwtRepository.StoreJtiAsync(id, userId);
		return tokenHandler.WriteToken(token);
	}

	public async Task<ClaimsPrincipal?> ValidateToken(string token, bool refresh = false){
		var tokenHandler = new JwtSecurityTokenHandler();

		try{
			if(refresh){
				var read = tokenHandler.ReadJwtToken(token);
				var jti = read.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Jti);
				var exists = await _jwtRepository.FindJtiAsync(jti.Value) ??
				             throw new Exception("Refresh token not in database");
			}

			var key = Encoding.ASCII.GetBytes(refresh ? _refreshSecret : _jwtSecret);
			var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = false,
				ValidateAudience = false,
				ClockSkew = TimeSpan.Zero
			}, out var validatedToken);
			return principal;
		}
		catch{
			return null;
		}
	}

	public async Task<string?> GetUsernameFromToken(string token, bool refresh = false){
		var principal = await ValidateToken(token, refresh);
		return principal?.FindFirst("username")?.Value ??
		       principal?.FindFirst(ClaimTypes.Name)?.Value;
	}

	public async Task DeleteRefreshToken(string token){
		var tokenHandler = new JwtSecurityTokenHandler();
		var read = tokenHandler.ReadJwtToken(token);
		var jti = read.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Jti);
		if(jti == null)
			return;

		await _jwtRepository.FindJtiAndDeleteAsync(jti.Value);
	}
}