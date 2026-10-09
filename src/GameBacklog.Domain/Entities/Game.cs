using GameBacklog.Domain.Enums;

namespace GameBacklog.Domain.Entities
{
    public class Game
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Title { get; set; }
        public Platform Platform { get; set; }
        public GameStatus Status { get; set; } = GameStatus.UNPLAYED;
        public int? Rating { get; set; }
        public string? Notes { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
