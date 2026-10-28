using GameBacklog.Api.Exceptions;
using GameBacklog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.Extensions
{
    public static class EntityLookupExtensions
    {
        public static async Task<Game> FindUserGameAsync(this IQueryable<Game> games, Guid userId, Guid gameId) =>
            await games.SingleOrDefaultAsync(g => g.Id == gameId && g.UserId == userId)
            ?? throw new NotFoundException("Game not found.");

        public static async Task EnsureGameExistsAsync(this IQueryable<Game> games, Guid userId, Guid gameId)
        {
            var exists = await games.AnyAsync(g => g.Id == gameId && g.UserId == userId);
            if (!exists) throw new NotFoundException("Game not found.");
        }

        public static async Task<Objective> FindGameObjectiveAsync(this IQueryable<Objective> objectives, Guid gameId, Guid objectiveId) =>
            await objectives.SingleOrDefaultAsync(o => o.Id == objectiveId && o.GameId == gameId)
            ?? throw new NotFoundException("Objective not found.");
    }
}
