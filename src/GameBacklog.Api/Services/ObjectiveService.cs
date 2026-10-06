using GameBacklog.Api.Dtos;
using GameBacklog.Api.Services.Interfaces;
using GameBacklog.Domain.Entities;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.Services {
    public class ObjectiveService : IObjectiveService
    {
        private readonly AppDbContext _context;

        public ObjectiveService(AppDbContext context) { _context = context; }

        public Task<bool> GameExistsAsync(Guid userId, Guid gameId) =>
            _context.Games.AnyAsync(g => g.Id == gameId && g.UserId == userId);

        public async Task<ObjectiveResponse?> CreateObjectiveAsync(Guid gameId, CreateObjectiveRequest request)
        {
            var label = request.Label.Trim();

            var labelExists = await _context.Objectives.AnyAsync(o => o.GameId == gameId && o.Label == label);
            if (labelExists) return null;

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

        public async Task<List<ObjectiveResponse>> GetObjectivesAsync(Guid gameId)
        {
            var objectives = await _context.Objectives
                .Where(o => o.GameId == gameId)
                .OrderBy(o => o.Position)
                .ToListAsync();

            return objectives.Select(ObjectiveResponse.From).ToList();
        }

        public async Task<ObjectiveResponse?> UpdateObjectiveAsync(Guid gameId, Guid objectiveId, UpdateObjectiveRequest request)
        {
            var objective = await FindObjectiveAsync(gameId, objectiveId);
            if (objective is null) return null;

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

        public async Task<bool> DeleteObjectiveAsync(Guid gameId, Guid objectiveId)
        {
            var objective = await FindObjectiveAsync(gameId, objectiveId);
            if (objective is null) return false;

            _context.Objectives.Remove(objective);
            await _context.SaveChangesAsync();

            return true;
        }

        private Task<Objective?> FindObjectiveAsync(Guid gameId, Guid objectiveId) =>
            _context.Objectives.SingleOrDefaultAsync(o => o.Id == objectiveId && o.GameId == gameId);
    }
}
