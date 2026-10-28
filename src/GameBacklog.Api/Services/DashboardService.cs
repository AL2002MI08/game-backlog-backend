using GameBacklog.Api.Dtos;
using GameBacklog.Api.Extensions;
using GameBacklog.Api.Services.Interfaces;
using GameBacklog.Domain.Enums;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.Services {
    public class DashboardService : IDashboardService
    {
        private const int RecentGamesCount = 5;

        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context) { _context = context; }

        public async Task<DashboardResponse> GetDashboardAsync(Guid userId)
        {
            var games = await _context.Games
                .Where(g => g.UserId == userId)
                .ToListRows(_context.Objectives)
                .ToListAsync();

            var gamesByStatus = Enum.GetValues<GameStatus>()
                .ToDictionary(status => status.ToString(), status => games.Count(g => g.Status == status));

            var ratings = games.Where(g => g.Rating.HasValue).Select(g => g.Rating!.Value).ToList();
            double? averageRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : null;

            return new DashboardResponse(
                TotalGames: games.Count,
                GamesByStatus: gamesByStatus,
                FullyCompleteGames: games.Count(g => g.TotalObjectives > 0 && g.CompletedObjectives == g.TotalObjectives),
                GamesWithNoObjectives: games.Count(g => g.TotalObjectives == 0),
                AverageRating: averageRating,
                RecentlyUpdatedGames: games
                    .OrderByDescending(g => g.UpdatedAt)
                    .Take(RecentGamesCount)
                    .Select(g => g.ToResponse())
                    .ToList());
        }
    }
}
