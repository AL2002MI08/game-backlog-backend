namespace GameBacklog.Api.Dtos
{
    public record LoginResponse(string Token, Guid UserId, string Email, DateTime ExpiresAt);
    public record RegisterResponse(bool Succeeded, string Message);
}
