using System.Security.Claims;
using GameBacklog.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameBacklog.Api.Controllers {
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController(IDashboardService dashboardService) : ControllerBase
    {
        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await dashboardService.GetDashboardAsync(UserId));
        }
    }
}
