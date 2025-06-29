using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fichte.Providers
{
    public class JwtTokenProvider
    {
        public static string CreateToken(string secureKey, int expiration, string? subject = null, string? role = null)
        {
            var tokenKey = Encoding.UTF8.GetBytes(secureKey);
            var tokenDescriptor = new SecurityTokenDescriptor
             {
                 Expires = DateTime.UtcNow.AddMinutes(expiration),
                 SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
             };

            List<Claim> claims = new();

            if (!string.IsNullOrEmpty(subject))
            {
                claims.Add(new Claim(ClaimTypes.Name, subject));
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));
            }

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            tokenDescriptor.Subject = new ClaimsIdentity(claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static ClaimsPrincipal? ValidateTokenAndGetPrincipal(string token, string secureKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(secureKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}