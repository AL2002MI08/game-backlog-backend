using Microsoft.AspNetCore.Mvc;
using GameBacklog.Api.Dtos;
using GameBacklog.Infrastructure.Persistence;

namespace GameBacklog.Api.Controllers {
    [ApiController]
    [Route("api/auth")]
    public class AuthController: ControllerBase {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context) {
            _context = context;
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request){
            return Ok();
        }
        
    }
}