using System.ComponentModel.DataAnnotations;
using GameBacklog.Domain.Entities;

namespace GameBacklog.Api.Dtos
{
    public record CreateObjectiveRequest
    {
        [Required, MaxLength(200)]
        public required string Label { get; init; }

        [Range(0, int.MaxValue)]
        public int? Position { get; init; }
    }

    public record UpdateObjectiveRequest
    {
        public bool? Completed { get; init; }

        [Range(0, int.MaxValue)]
        public int? Position { get; init; }
    }

    public record ObjectiveResponse(
        Guid Id, Guid GameId, string Label, bool Completed, int Position,
        DateTime? CompletedAt, DateTime CreatedAt, DateTime UpdatedAt)
    {
        public static ObjectiveResponse From(Objective o) => new(
            o.Id, o.GameId, o.Label, o.Completed, o.Position,
            o.CompletedAt, o.CreatedAt, o.UpdatedAt);
    }
}
