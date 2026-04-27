using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkOrderApp.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WorkOrderApp.Helpers.Auth
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _settings;

        public JwtService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GenerateToken(string id, string name, string email, string role)
        {
            var expiry = DateTime.UtcNow.AddMinutes(_settings.DurationInMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,           id),
                new Claim(JwtRegisteredClaimNames.Jti,           Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email,         email),
                new Claim(ClaimTypes.Sid,                        id),
                new Claim(ClaimTypes.Name,                       name),
                new Claim(ClaimTypes.Role,                       role),
                new Claim(ClaimTypes.NameIdentifier,             id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var token = new JwtSecurityToken(
                issuer:             _settings.Issuer,
                audience:           _settings.Audience,
                claims:             claims,
                expires:            expiry,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
