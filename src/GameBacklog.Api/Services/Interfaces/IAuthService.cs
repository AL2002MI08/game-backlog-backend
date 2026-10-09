using GameBacklog.Api.Dtos;

namespace GameBacklog.Api.Services.Interfaces {
    public interface IAuthService {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
