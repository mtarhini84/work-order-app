using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WorkOrderApp.Tests.Infrastructure
{
    /// <summary>
    /// Builds valid JWT tokens that match the test appsettings configuration.
    /// </summary>
    public static class TokenHelper
    {
        // Must match appsettings.json JwtSettings values used by TestWebAppFactory.
        private const string Key      = "TemplateApp__HelloWorld__JWTSecretKey__$$$$";
        private const string Issuer   = "<issuer>";
        private const string Audience = "<audience>";

        public static string GenerateToken(string userId, string role)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Sid,  userId),
                new Claim(ClaimTypes.Role, role),
            };

            var token = new JwtSecurityToken(
                issuer:             Issuer,
                audience:           Audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
