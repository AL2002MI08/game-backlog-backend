using System.Security.Claims;
using GameBacklog.Api.Dtos;
using GameBacklog.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameBacklog.Api.Controllers {
    [ApiController]
    [Route("api/games/{gameId:guid}/objectives")]
    [Authorize]
    public class ObjectiveController(IObjectiveService objectiveService) : ControllerBase
    {
        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create(Guid gameId, [FromBody] CreateObjectiveRequest request)
        {
            var objective = await objectiveService.CreateObjectiveAsync(UserId, gameId, request);
            return StatusCode(StatusCodes.Status201Created, objective);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid gameId)
        {
            return Ok(await objectiveService.GetObjectivesAsync(UserId, gameId));
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid gameId, Guid id, [FromBody] UpdateObjectiveRequest request)
        {
            return Ok(await objectiveService.UpdateObjectiveAsync(UserId, gameId, id, request));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid gameId, Guid id)
        {
            await objectiveService.DeleteObjectiveAsync(UserId, gameId, id);
            return NoContent();
        }
    }
}
