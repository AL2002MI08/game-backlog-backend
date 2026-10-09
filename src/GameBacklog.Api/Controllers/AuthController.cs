using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using GameBacklog.Api.Dtos;
using GameBacklog.Api.Extensions;
using GameBacklog.Api.Services.Interfaces;

namespace GameBacklog.Api.Controllers {
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting(RateLimitExtension.AuthPolicy)]
    public class AuthController(IAuthService authService) : ControllerBase {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return Ok(await authService.LoginAsync(request));
        }
    }
}