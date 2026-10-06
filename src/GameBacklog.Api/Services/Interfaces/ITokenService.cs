using GameBacklog.Domain.Entities;

namespace GameBacklog.Api.Services.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) CreateToken(User user);
    }
}
