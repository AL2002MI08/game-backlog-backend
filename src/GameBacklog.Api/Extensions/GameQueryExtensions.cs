using GameBacklog.Api.Dtos;
using GameBacklog.Domain.Entities;
using GameBacklog.Domain.Enums;

namespace GameBacklog.Api.Extensions
{
    public class GameListRow
    {
        public Guid Id { get; init; }
        public required string Title { get; init; }
        public Platform Platform { get; init; }
        public GameStatus Status { get; init; }
        public int? Rating { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int TotalObjectives { get; init; }
        public int CompletedObjectives { get; init; }

        public GameListResponse ToResponse() => new(
            Id, Title, Platform, Status, Rating, UpdatedAt,
            CompletionProgressResponse.From(TotalObjectives, CompletedObjectives));
    }

    public static class GameQueryExtensions
    {
        public static IQueryable<Game> ApplyFilters(this IQueryable<Game> games, GameQueryParameters query, IQueryable<Objective> objectives)
        {
            var search = query.Search?.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
                games = games.Where(g => g.Title.Contains(search));

            if (query.Platform.HasValue)
                games = games.Where(g => g.Platform == query.Platform.Value);

            if (query.Status.HasValue)
                games = games.Where(g => g.Status == query.Status.Value);

            if (query.MinRating.HasValue)
                games = games.Where(g => g.Rating >= query.MinRating.Value);

            if (query.MaxRating.HasValue)
                games = games.Where(g => g.Rating <= query.MaxRating.Value);

            if (query.HasObjectives.HasValue)
                games = games.Where(g => objectives.Any(o => o.GameId == g.Id) == query.HasObjectives.Value);

            if (query.IsComplete.HasValue)
            {
                if (query.IsComplete.Value)
                    games = games.Where(g =>
                        objectives.Any(o => o.GameId == g.Id) &&
                        !objectives.Any(o => o.GameId == g.Id && !o.Completed));
                else
                    games = games.Where(g =>
                        !objectives.Any(o => o.GameId == g.Id) ||
                        objectives.Any(o => o.GameId == g.Id && !o.Completed));
            }

            return games;
        }

        public static IQueryable<GameListRow> ToListRows(this IQueryable<Game> games, IQueryable<Objective> objectives) =>
            games.Select(g => new GameListRow
            {
                Id = g.Id,
                Title = g.Title,
                Platform = g.Platform,
                Status = g.Status,
                Rating = g.Rating,
                UpdatedAt = g.UpdatedAt,
                TotalObjectives = objectives.Count(o => o.GameId == g.Id),
                CompletedObjectives = objectives.Count(o => o.GameId == g.Id && o.Completed),
            });

        public static IQueryable<GameListRow> ApplySort(this IQueryable<GameListRow> rows, GameQueryParameters query)
        {
            var isAsc = string.Equals(query.SortOrder, "asc", StringComparison.OrdinalIgnoreCase);

            return query.SortBy?.ToLower() switch
            {
                "title" => isAsc ? rows.OrderBy(r => r.Title) : rows.OrderByDescending(r => r.Title),
                "rating" => isAsc ? rows.OrderBy(r => r.Rating) : rows.OrderByDescending(r => r.Rating),
                "progress" or "completion" => isAsc
                    ? rows.OrderBy(r => r.TotalObjectives == 0 ? -1.0 : (double)r.CompletedObjectives / r.TotalObjectives)
                    : rows.OrderByDescending(r => r.TotalObjectives == 0 ? -1.0 : (double)r.CompletedObjectives / r.TotalObjectives),
                _ => isAsc ? rows.OrderBy(r => r.UpdatedAt) : rows.OrderByDescending(r => r.UpdatedAt),
            };
        }
    }
}
