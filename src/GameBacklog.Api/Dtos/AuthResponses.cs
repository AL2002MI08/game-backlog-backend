namespace GameBacklog.Api.Dtos
{
    public record LoginResponse(string Token, DateTime ExpiresAt);
    public record RegisterResponse(bool Succeeded, string Message);
    public record UserProfileResponse(Guid Id, string Email);
}
