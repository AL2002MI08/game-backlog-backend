using GameBacklog.Api.Dtos;
using GameBacklog.Api.Events;
using GameBacklog.Api.Exceptions;
using GameBacklog.Api.Extensions;
using GameBacklog.Api.Services.Interfaces;
using GameBacklog.Domain.Entities;
using GameBacklog.Domain.Enums;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.Services {
    public class GameService : IGameService
    {
        private readonly AppDbContext _context;
        private readonly IEventPublisher _events;

        public GameService(AppDbContext context, IEventPublisher events) { _context = context; _events = events; }

        public async Task<GameResponse> CreateGameAsync(Guid userId, CreateGameRequest request)
        {
            var title = request.Title.Trim();

            if (request.Status == GameStatus.UNPLAYED && request.Rating is not null)
                throw new BadRequestException("Rating cannot be set on an unplayed game.");

            var exists = await _context.Games.AnyAsync(g =>
                g.UserId == userId && g.Title == title && g.Platform == request.Platform);
            if (exists) throw new ConflictException("You already have this game on this platform.");

            var now = DateTime.UtcNow;
            var game = new Game
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Platform = request.Platform,
                Rating = request.Rating,
                Notes = request.Notes,
                CreatedAt = now,
                UpdatedAt = now,
            };

            game.SetStatus(request.Status, now);
            _context.Games.Add(game);

            if (request.Objectives is { Count: > 0 })
                _context.Objectives.AddRange(request.Objectives.ToEntities(game.Id, now));

            await _context.SaveChangesAsync();

            return GameResponse.From(game);
        }

        public Task<PagedResponse<GameListResponse>> GetGamesAsync(Guid userId, GameQueryParameters query) =>
            _context.Games
                .Where(g => g.UserId == userId)
                .ApplyFilters(query, _context.Objectives)
                .ToListRows(_context.Objectives)
                .ApplySort(query)
                .ToPagedResponseAsync(query, row => row.ToResponse());

        public async Task<GameDetailResponse> GetGameAsync(Guid userId, Guid gameId)
        {
            var game = await _context.Games.FindUserGameAsync(userId, gameId);

            var objectives = await _context.Objectives
                .AsNoTracking()
                .Where(o => o.GameId == gameId)
                .OrderBy(o => o.Position)
                .ToListAsync();

            var total = objectives.Count;
            var completed = objectives.Count(o => o.Completed);

            return GameDetailResponse.From(game, objectives.Select(ObjectiveResponse.From).ToList(), total, completed);
        }

        public Task<GameResponse> UpdateGameAsync(Guid userId, Guid gameId, UpdateGameRequest request) =>
            SaveGameUpdatesAsync(userId, gameId, request, isPartial: false);

        public Task<GameResponse> PartialUpdateGameAsync(Guid userId, Guid gameId, UpdateGameRequest request) =>
            SaveGameUpdatesAsync(userId, gameId, request, isPartial: true);

        private async Task<GameResponse> SaveGameUpdatesAsync(Guid userId, Guid gameId, UpdateGameRequest request, bool isPartial)
        {
            var game = await _context.Games.FindUserGameAsync(userId, gameId);
            var now = DateTime.UtcNow;

            var title = request.Title?.Trim() ?? (isPartial ? game.Title : null);
            var platform = request.Platform ?? (isPartial ? game.Platform : default(Platform?));

            if (title is not null && platform.HasValue && (title != game.Title || platform.Value != game.Platform))
            {
                var exists = await _context.Games.AnyAsync(g =>
                    g.UserId == userId && g.Id != gameId && g.Title == title && g.Platform == platform.Value);
                if (exists) throw new ConflictException("You already have this game on this platform.");
                game.Title = title;
                game.Platform = platform.Value;
            }

            var statusChanged = RecordStatusChange(game, request.Status, userId, now, request.Comment);
            if (game.Status == GameStatus.UNPLAYED && request.Rating is not null)
                throw new BadRequestException("Rating cannot be set on an unplayed game.");

            if (!isPartial || request.Rating is not null)
                game.Rating = game.Status == GameStatus.UNPLAYED ? null : request.Rating;

            if (!isPartial || request.Notes is not null)
                game.Notes = request.Notes;

            game.UpdatedAt = now;

            await SaveAndPublishAsync(statusChanged);
            return GameResponse.From(game);
        }

        public async Task<UpdateGameStatusResponse> UpdateGameStatusAsync(Guid userId, Guid gameId, UpdateGameStatusRequest request)
        {
            var game = await _context.Games.FindUserGameAsync(userId, gameId);
            var now = DateTime.UtcNow;

            var statusChanged = RecordStatusChange(game, request.Status, userId, now, request.Comment);
            game.UpdatedAt = now;

            await SaveAndPublishAsync(statusChanged);

            var hasUntickedObjectives = await _context.Objectives.AnyAsync(o => o.GameId == gameId && !o.Completed);
            var hasOutstandingObjectives = request.Status == GameStatus.FINISHED && hasUntickedObjectives;

            return new UpdateGameStatusResponse(GameResponse.From(game), hasOutstandingObjectives);
        }

        public async Task<CompletionProgressResponse> GetGameProgressAsync(Guid userId, Guid gameId)
        {
            await _context.Games.EnsureGameExistsAsync(userId, gameId);

            var counts = await _context.Objectives
                .Where(o => o.GameId == gameId)
                .GroupBy(o => o.GameId)
                .Select(g => new { Total = g.Count(), Completed = g.Count(o => o.Completed) })
                .SingleOrDefaultAsync();

            return CompletionProgressResponse.From(counts?.Total ?? 0, counts?.Completed ?? 0);
        }

        public async Task<PagedResponse<GameStatusHistoryResponse>> GetGameHistoryAsync(Guid userId, Guid gameId, PaginationParameters pagination)
        {
            await _context.Games.EnsureGameExistsAsync(userId, gameId);

            return await _context.GameStatusHistories
                .AsNoTracking()
                .Where(h => h.GameId == gameId)
                .OrderByDescending(h => h.ChangedAt)
                .ToPagedResponseAsync(pagination, h => new GameStatusHistoryResponse
                {
                    GameId = h.GameId.ToString(),
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    ChangedBy = h.ChangedBy.ToString(),
                    ChangedAt = h.ChangedAt,
                    Comment = h.Comment
                });
        }

        public async Task DeleteGameAsync(Guid userId, Guid gameId)
        {
            var game = await _context.Games.FindUserGameAsync(userId, gameId);

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }

        private async Task SaveAndPublishAsync(GameStatusChangedEvent? statusChanged)
        {
            await _context.SaveChangesAsync();

            if (statusChanged is not null) await _events.PublishAsync(statusChanged);
        }

        private GameStatusChangedEvent? RecordStatusChange(Game game, GameStatus? newStatus, Guid userId, DateTime now, string? comment)
        {
            if (newStatus is null) return null;

            var currentStatus = game.Status;
            var requestedStatus = newStatus.Value;

            if (currentStatus == requestedStatus)
                throw new ConflictException($"Game is already in status {requestedStatus}.");

            if (!game.CanTransitionTo(requestedStatus))
                throw new ConflictException($"Cannot transition from {currentStatus} to {requestedStatus}.");

            _context.GameStatusHistories.Add(new GameStatusHistory
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                PreviousStatus = game.Status,
                NewStatus = requestedStatus,
                ChangedBy = userId,
                ChangedAt = now,
                Comment = comment
            });

            game.SetStatus(requestedStatus, now);

            return new GameStatusChangedEvent(game.Id, userId, currentStatus, requestedStatus, now, comment);
        }
    }
}
