using System.ComponentModel.DataAnnotations;
using GameBacklog.Domain.Entities;
using GameBacklog.Domain.Enums;

namespace GameBacklog.Api.Dtos
{
    public record CreateGameRequest
    {
        [Required, MaxLength(100)]
        public required string Title { get; init; }

        [EnumDataType(typeof(Platform))]
        public required Platform Platform { get; init; }

        [EnumDataType(typeof(GameStatus))]
        public GameStatus Status { get; init; } = GameStatus.UNPLAYED;

        [Range(1, 10)]
        public int? Rating { get; init; }

        [MaxLength(2000)]
        public string? Notes { get; init; }

        public List<CreateObjectiveRequest>? Objectives { get; init; }
    }

    public record UpdateGameRequest
    {
        
        public string? Title {get; init;}

        [EnumDataType(typeof(GameStatus))]
        public GameStatus? Status { get; init; }

        [EnumDataType(typeof(Platform))]
        public Platform? Platform {get; init;}

        [Range(1, 10)]
        public int? Rating { get; init; }

        [MaxLength(2000)]
        public string? Notes { get; init; }

        public string? Comment { get; init; }
    }

    public record GameResponse(
        Guid Id, string Title, Platform Platform, GameStatus Status,
        int? Rating, string? Notes, DateTime? StartedAt, DateTime? FinishedAt,
        DateTime CreatedAt, DateTime UpdatedAt)
    {
        public static GameResponse From(Game g) => new(
            g.Id, g.Title, g.Platform, g.Status, g.Rating, g.Notes,
            g.StartedAt, g.FinishedAt, g.CreatedAt, g.UpdatedAt);
    }

    public record GameListResponse(
        Guid Id, string Title, Platform Platform, GameStatus Status,
        int? Rating, DateTime UpdatedAt, CompletionProgressResponse Progress)
    {
        public static GameListResponse From(Game g, int total, int completed) => new(
            g.Id, g.Title, g.Platform, g.Status, g.Rating, g.UpdatedAt,
            CompletionProgressResponse.From(total, completed));
    }

    public record GameDetailResponse(
        Guid Id, string Title, Platform Platform, GameStatus Status,
        int? Rating, string? Notes, DateTime? StartedAt, DateTime? FinishedAt,
        DateTime CreatedAt, DateTime UpdatedAt,
        List<ObjectiveResponse> Objectives,
        CompletionProgressResponse Progress)
    {
        public static GameDetailResponse From(Game g, List<ObjectiveResponse> objectives, int total, int completed) => new(
            g.Id, g.Title, g.Platform, g.Status, g.Rating, g.Notes,
            g.StartedAt, g.FinishedAt, g.CreatedAt, g.UpdatedAt,
            objectives,
            CompletionProgressResponse.From(total, completed));
    }

    public record UpdateGameStatusResponse(
        GameResponse Game,
        bool HasOutstandingObjectives);

    public record CompletionProgressResponse(int Total, int Completed, int? Percentage)
    {
        public static CompletionProgressResponse From(int total, int completed) =>
            new(
                Total: total,
                Completed: completed,
                Percentage: total == 0 ? null : (int?)Math.Round((double)completed / total * 100.0)
            );
    }

    public record UpdateGameStatusRequest
    {
        [Required]
        [EnumDataType(typeof(GameStatus))]
        public required GameStatus Status { get; init; }

        public string? Comment { get; init; }
    }

    public record GameQueryParameters : PaginationParameters
    {
        public string? Search { get; init; }
        public GameStatus? Status { get; init; }
        public Platform? Platform { get; init; }
        public int? MinRating { get; init; }
        public int? MaxRating { get; init; }
        public bool? HasObjectives { get; init; }
        public bool? IsComplete { get; init; }
        public string? SortBy { get; init; } // "title", "rating", "progress", "updatedat"
        public string? SortOrder { get; init; } // "asc" or "desc"
    }

}
