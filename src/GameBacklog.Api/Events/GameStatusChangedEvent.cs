using GameBacklog.Domain.Enums;

namespace GameBacklog.Api.Events
{
    public record GameStatusChangedEvent(
        Guid GameId,
        Guid UserId,
        GameStatus PreviousStatus,
        GameStatus NewStatus,
        DateTime ChangedAt,
        string? Comment);
}
