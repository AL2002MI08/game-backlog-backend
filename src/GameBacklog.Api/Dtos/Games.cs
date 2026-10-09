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
    }

    public record UpdateGameRequest
    {
        [EnumDataType(typeof(GameStatus))]
        public GameStatus? Status { get; init; }

        [Range(1, 10)]
        public int? Rating { get; init; }

        [MaxLength(2000)]
        public string? Notes { get; init; }
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
}
