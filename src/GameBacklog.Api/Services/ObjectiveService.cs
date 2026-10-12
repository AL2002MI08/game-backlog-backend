using GameBacklog.Api.Dtos;
using GameBacklog.Api.Exceptions;
using GameBacklog.Api.Extensions;
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
            await _context.Games.EnsureGameExistsAsync(userId, gameId);
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
            await _context.Games.EnsureGameExistsAsync(userId, gameId);

            var objectives = await _context.Objectives
                .AsNoTracking()
                .Where(o => o.GameId == gameId)
                .OrderBy(o => o.Position)
                .ToListAsync();

            return objectives.Select(ObjectiveResponse.From).ToList();
        }

        public async Task<ObjectiveResponse> PartialUpdateObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId, UpdateObjectiveRequest request)
        {
            var objective = await FindObjectiveAsync(userId, gameId, objectiveId);
            var now = DateTime.UtcNow;

            if (request.Label is not null)
            {
                var label = request.Label.Trim();
                if (label != objective.Label)
                {
                    var labelExists = await _context.Objectives.AnyAsync(o =>
                        o.GameId == gameId && o.Id != objectiveId && o.Label == label);
                    if (labelExists) throw new ConflictException("This game already has an objective with this label.");
                }
                objective.Label = label;
            }

            if (request.Completed is not null)
            {
                objective.Completed = request.Completed.Value;
                objective.CompletedAt = objective.Completed ? objective.CompletedAt ?? now : null;
            }
            if (request.Position is not null)
            {
                await ReorderObjectiveAsync(gameId, objective, request.Position.Value);
            }
            objective.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return ObjectiveResponse.From(objective);
        }

        public async Task<ObjectiveResponse> ReplaceObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId, ReplaceObjectiveRequest request)
        {
            var objective = await FindObjectiveAsync(userId, gameId, objectiveId);
            var now = DateTime.UtcNow;

            var label = request.Label.Trim();
            if (label != objective.Label)
            {
                var labelExists = await _context.Objectives.AnyAsync(o =>
                    o.GameId == gameId && o.Id != objectiveId && o.Label == label);
                if (labelExists) throw new ConflictException("This game already has an objective with this label.");
            }

            objective.Label = label;
            objective.UpdatedAt = now;

            if (request.Position is not null)
            {
                await ReorderObjectiveAsync(gameId, objective, request.Position.Value);
            }

            await _context.SaveChangesAsync();

            return ObjectiveResponse.From(objective);
        }

        public async Task<ObjectiveResponse> CompleteObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId)
        {
            var objective = await FindObjectiveAsync(userId, gameId, objectiveId);

            if (objective.Completed) throw new BadRequestException("Objective is already completed.");

            var now = DateTime.UtcNow;
            objective.Completed = true;
            objective.CompletedAt = now;
            objective.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return ObjectiveResponse.From(objective);
        }

        public async Task<ObjectiveResponse> ReopenObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId)
        {
            var objective = await FindObjectiveAsync(userId, gameId, objectiveId);

            if (!objective.Completed) throw new BadRequestException("Objective is not completed.");

            var now = DateTime.UtcNow;
            objective.Completed = false;
            objective.CompletedAt = null;
            objective.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return ObjectiveResponse.From(objective);
        }

        public async Task<List<ObjectiveResponse>> ReorderObjectivesAsync(Guid userId, Guid gameId, ReorderObjectivesRequest request)
        {
            await _context.Games.EnsureGameExistsAsync(userId, gameId);

            var objectives = await _context.Objectives
                .Where(o => o.GameId == gameId)
                .ToListAsync();

            if (request.ObjectiveIds.Count != objectives.Count)
                throw new BadRequestException("The list must contain exactly one entry per objective.");

            var gameObjectiveIds = objectives.Select(o => o.Id).ToHashSet();
            if (!request.ObjectiveIds.All(id => gameObjectiveIds.Contains(id)))
                throw new BadRequestException("One or more objective IDs do not belong to this game.");

            var objectiveMap = objectives.ToDictionary(o => o.Id);
            var orderedList = request.ObjectiveIds.Select(id => objectiveMap[id]).ToList();

            orderedList.RenumberPositions(DateTime.UtcNow);
            await _context.SaveChangesAsync();

            return orderedList.Select(ObjectiveResponse.From).ToList();
        }

        public async Task DeleteObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId)
        {
            var objective = await FindObjectiveAsync(userId, gameId, objectiveId);

            _context.Objectives.Remove(objective);
            await _context.SaveChangesAsync();
        }

        private async Task<Objective> FindObjectiveAsync(Guid userId, Guid gameId, Guid objectiveId)
        {
            await _context.Games.EnsureGameExistsAsync(userId, gameId);
            return await _context.Objectives.FindGameObjectiveAsync(gameId, objectiveId);
        }

        private async Task ReorderObjectiveAsync(Guid gameId, Objective objective, int newPosition)
        {
            var list = await _context.Objectives
                .Where(o => o.GameId == gameId)
                .OrderBy(o => o.Position)
                .ToListAsync();

            list.Reorder(objective, newPosition, DateTime.UtcNow);
        }
    }
}
