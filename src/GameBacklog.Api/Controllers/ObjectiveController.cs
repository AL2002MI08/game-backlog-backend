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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid gameId, Guid id)
        {
            return Ok(await objectiveService.GetObjectiveAsync(UserId, gameId, id));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Replace(Guid gameId, Guid id, [FromBody] ReplaceObjectiveRequest request)
        {
            return Ok(await objectiveService.ReplaceObjectiveAsync(UserId, gameId, id, request));
        }

        [HttpPatch("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid gameId, Guid id)
        {
            return Ok(await objectiveService.CompleteObjectiveAsync(UserId, gameId, id));
        }

        [HttpPatch("{id:guid}/reopen")]
        public async Task<IActionResult> Reopen(Guid gameId, Guid id)
        {
            return Ok(await objectiveService.ReopenObjectiveAsync(UserId, gameId, id));
        }

        [HttpPut("order")]
        public async Task<IActionResult> Reorder(Guid gameId, [FromBody] ReorderObjectivesRequest request)
        {
            return Ok(await objectiveService.ReorderObjectivesAsync(UserId, gameId, request));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid gameId, Guid id)
        {
            await objectiveService.DeleteObjectiveAsync(UserId, gameId, id);
            return NoContent();
        }
    }
}

