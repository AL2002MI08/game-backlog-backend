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
        public string? CoverImage { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<Objective> Objectives { get; set; } = [];

        public bool CanTransitionTo(GameStatus targetStatus) => Status switch
        {
            GameStatus.UNPLAYED => targetStatus is GameStatus.PLAYING or GameStatus.FINISHED or GameStatus.ABANDONED,
            GameStatus.PLAYING => targetStatus is GameStatus.FINISHED or GameStatus.ABANDONED or GameStatus.UNPLAYED,
            GameStatus.FINISHED => targetStatus is GameStatus.PLAYING or GameStatus.ABANDONED,
            GameStatus.ABANDONED => targetStatus is GameStatus.PLAYING or GameStatus.FINISHED,
            _ => false
        };

        public void SetStatus(GameStatus newStatus, DateTime now)
        {
            if (newStatus == GameStatus.UNPLAYED)
            {
                StartedAt = null;
                Rating = null;
            }
            else if (newStatus != GameStatus.ABANDONED)
            {
                StartedAt ??= now;
            }

            FinishedAt = newStatus == GameStatus.FINISHED ? (FinishedAt ?? now) : null;
            Status = newStatus;
        }
    }
}
