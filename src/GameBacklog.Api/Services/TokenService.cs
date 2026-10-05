using System.Security.Claims;
using System.Text;
using GameBacklog.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GameBacklog.Api.Services
{
    public class TokenService(IConfiguration config) : ITokenService
    {
        public (string Token, DateTime ExpiresAt) CreateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT_KEY"]!));
            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(config["JWT_EXPIRES_MINUTES"] ?? "30"));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                ]),
                Issuer = config["JWT_ISSUER"],
                Audience = config["JWT_AUDIENCE"],
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            };

            var token = new JsonWebTokenHandler().CreateToken(descriptor);
            return (token, expiresAt);
        }
    }
}
