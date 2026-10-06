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
            return game is null
                ? Conflict(new { message = "You already have this game on this platform." })
                : StatusCode(StatusCodes.Status201Created, game);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await gameService.GetGamesAsync(UserId));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var game = await gameService.GetGameAsync(UserId, id);
            return game is null ? NotFound(new { message = "Game not found." }) : Ok(game);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameRequest request)
        {
            var game = await gameService.UpdateGameAsync(UserId, id, request);
            return game is null ? NotFound(new { message = "Game not found." }) : Ok(game);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await gameService.DeleteGameAsync(UserId, id);
            return deleted ? NoContent() : NotFound(new { message = "Game not found." });
        }
    }
}
