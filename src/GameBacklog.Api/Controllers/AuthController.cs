using Microsoft.AspNetCore.Mvc;
using GameBacklog.Api.Dtos;
using GameBacklog.Api.Services;

namespace GameBacklog.Api.Controllers {
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await authService.RegisterAsync(request);
            return result.Succeeded
                ? StatusCode(StatusCodes.Status201Created, result)
                : Conflict(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await authService.LoginAsync(request);
            return result is null
                ? Unauthorized(new { message = "Invalid email or password." })
                : Ok(result);
        }
    }
}