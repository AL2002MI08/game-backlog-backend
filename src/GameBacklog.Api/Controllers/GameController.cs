using System.Security.Claims;
using GameBacklog.Api.Dtos;
using GameBacklog.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameBacklog.Api.Controllers {
    [ApiController]
    [Route("api/games")]
    [Authorize]
    public class GameController(IGameService gameService) : ControllerBase
    {
        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGameRequest request)
        {
            var game = await gameService.CreateGameAsync(UserId, request);
            return StatusCode(StatusCodes.Status201Created, game);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await gameService.GetGamesAsync(UserId));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await gameService.GetGameAsync(UserId, id));
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameRequest request)
        {
            return Ok(await gameService.UpdateGameAsync(UserId, id, request));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await gameService.DeleteGameAsync(UserId, id);
            return NoContent();
        }
    }
}
