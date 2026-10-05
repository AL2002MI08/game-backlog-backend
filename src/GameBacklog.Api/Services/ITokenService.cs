using GameBacklog.Domain.Entities;

namespace GameBacklog.Api.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) CreateToken(User user);
    }
}
