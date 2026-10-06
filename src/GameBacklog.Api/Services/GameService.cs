using GameBacklog.Api.Dtos;
using GameBacklog.Api.Services.Interfaces;
using GameBacklog.Domain.Entities;
using GameBacklog.Domain.Enums;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.Services {
    public class GameService : IGameService
    {
        private readonly AppDbContext _context;

        public GameService(AppDbContext context) { _context = context; }

        public async Task<GameResponse?> CreateGameAsync(Guid userId, CreateGameRequest request)
        {
            var title = request.Title.Trim();

            var gameExists = await _context.Games.AnyAsync(g =>
                g.UserId == userId && g.Title == title && g.Platform == request.Platform);
            if (gameExists) return null;

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
            
            SetStatus(game, request.Status, now);
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            
            return GameResponse.From(game);
        }

        public async Task<List<GameResponse>> GetGamesAsync(Guid userId)
        {
            var games = await _context.Games
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.UpdatedAt)
                .ToListAsync();

            return games.Select(GameResponse.From).ToList();
        }

        public async Task<GameResponse?> GetGameAsync(Guid userId, Guid gameId)
        {
            var game = await FindGameAsync(userId, gameId);
            return game is null ? null : GameResponse.From(game);
        }

        public async Task<GameResponse?> UpdateGameAsync(Guid userId, Guid gameId, UpdateGameRequest request)
        {
            var game = await FindGameAsync(userId, gameId);
            if (game is null) return null;

            var now = DateTime.UtcNow;
            if (request.Status is not null) SetStatus(game, request.Status.Value, now);
            if (request.Rating is not null) game.Rating = request.Rating;
            if (request.Notes is not null) game.Notes = request.Notes;
            game.UpdatedAt = now;

            await _context.SaveChangesAsync();
            return GameResponse.From(game);
        }

        public async Task<bool> DeleteGameAsync(Guid userId, Guid gameId)
        {
            var game = await FindGameAsync(userId, gameId);
            if (game is null) return false;

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return true;
        }

        private Task<Game?> FindGameAsync(Guid userId, Guid gameId) =>
            _context.Games.SingleOrDefaultAsync(g => g.Id == gameId && g.UserId == userId);

        private static void SetStatus(Game game, GameStatus status, DateTime now)
        {
            if (status == GameStatus.UNPLAYED)
            {
                game.StartedAt = null;
            }
            else if (status != GameStatus.ABANDONED) { 
                game.StartedAt ??= now;
            }

            game.FinishedAt = status == GameStatus.FINISHED ? game.FinishedAt ?? now : null;
            game.Status = status;
        }
    }
}
