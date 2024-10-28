using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using expense_tracker.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace expense_tracker.Services;

public class JwtTokenService(IOptionsMonitor<JwtConfig> optionsMonitor)
{
    private readonly JwtConfig _jwtConfig = optionsMonitor.CurrentValue!;

    /*
     * The method below will generate the JSON WEB TOKEN (JWT)
     * it uses the Secret key, issuer and audience from jwt_config that is configured over
     * JwtConfig present in appsettings.cs file
     */
    public string GenerateJwtToken(AppUser user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        // getting the key
        var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

        // specifying all the necessary information here
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName ?? ""),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _jwtConfig.Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, _jwtConfig.Audience)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        return jwtToken;
    }
}