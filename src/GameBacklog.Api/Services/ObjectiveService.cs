using GameBacklog.Api.Dtos;
using GameBacklog.Api.Exceptions;
using GameBacklog.Api.Services.Interfaces;
using GameBacklog.Domain.Entities;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.Services {
    public class ObjectiveService : IObjectiveService
    {
        private readonly AppDbContext _context;

        public ObjectiveService(AppDbContext context) { _context = context; }

        public async Task<ObjectiveResponse> CreateObjectiveAsync(Guid userId, Guid gameId, CreateObjectiveRequest request)
        {
            await EnsureGameExistsAsync(userId, gameId);
            var label = request.Label.Trim();

            var labelExists = await _context.Objectives.AnyAsync(o => o.GameId == gameId && o.Label == label);
            if (labelExists) throw new ConflictException("This game already has an objective with this label.");

            var lastPosition = await _context.Objectives
                .Where(o => o.GameId == gameId)
                .MaxAsync(o => (int?)o.Position) ?? 0;

            var now = DateTime.UtcNow;
            var objective = new Objective
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                Label = label,
                Position = request.Position ?? lastPosition + 1,
                CreatedAt = now,
                UpdatedAt = now,
            };

            _context.Objectives.Add(objective);
            await _context.SaveChangesAsync();

            return ObjectiveResponse.From(objective);
        }

        public async Task<List<ObjectiveResponse>> GetObjectivesAsync(Guid userId, Guid gameId)
        {
            await EnsureGameExistsAsync(userId, gameId);

            var objectives = await _context.Objectives
                .Where(o => o.GameId == gameId)
                .OrderBy(o => o.Position)
                .ToListAsync();

            return objectives.Select(ObjectiveResponse.From).ToList();
        }

        public async Task<ObjectiveResponse> UpdateObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId, UpdateObjectiveRequest request)
        {
            var objective = await FindObjectiveAsync(userId, gameId, objectiveId);

            var now = DateTime.UtcNow;
            if (request.Completed is not null)
            {
                objective.Completed = request.Completed.Value;
                objective.CompletedAt = objective.Completed ? objective.CompletedAt ?? now : null;
            }
            if (request.Position is not null)
            {
                objective.Position = request.Position.Value;
            }
            objective.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return ObjectiveResponse.From(objective);
        }

        public async Task DeleteObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId)
        {
            var objective = await FindObjectiveAsync(userId, gameId, objectiveId);

            _context.Objectives.Remove(objective);
            await _context.SaveChangesAsync();
        }

        private async Task EnsureGameExistsAsync(Guid userId, Guid gameId)
        {
            var gameExists = await _context.Games.AnyAsync(g => g.Id == gameId && g.UserId == userId);
            if (!gameExists) throw new NotFoundException("Game not found.");
        }

        private async Task<Objective> FindObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId)
        {
            await EnsureGameExistsAsync(userId, gameId);

            return await _context.Objectives.SingleOrDefaultAsync(o => o.Id == objectiveId && o.GameId == gameId)
                ?? throw new NotFoundException("Objective not found.");
        }
    }
}
