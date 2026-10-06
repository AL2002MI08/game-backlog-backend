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
            if (!await objectiveService.GameExistsAsync(UserId, gameId)) return GameNotFound();

            var objective = await objectiveService.CreateObjectiveAsync(gameId, request);
            return objective is null
                ? Conflict(new { message = "This game already has an objective with this label." })
                : StatusCode(StatusCodes.Status201Created, objective);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid gameId)
        {
            if (!await objectiveService.GameExistsAsync(UserId, gameId)) return GameNotFound();

            return Ok(await objectiveService.GetObjectivesAsync(gameId));
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid gameId, Guid id, [FromBody] UpdateObjectiveRequest request)
        {
            if (!await objectiveService.GameExistsAsync(UserId, gameId)) return GameNotFound();

            var objective = await objectiveService.UpdateObjectiveAsync(gameId, id, request);
            return objective is null ? ObjectiveNotFound() : Ok(objective);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid gameId, Guid id)
        {
            if (!await objectiveService.GameExistsAsync(UserId, gameId)) return GameNotFound();

            var deleted = await objectiveService.DeleteObjectiveAsync(gameId, id);
            return deleted ? NoContent() : ObjectiveNotFound();
        }

        private NotFoundObjectResult GameNotFound() => NotFound(new { message = "Game not found." });
        private NotFoundObjectResult ObjectiveNotFound() => NotFound(new { message = "Objective not found." });
    }
}
