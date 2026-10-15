using GameBacklog.Domain.Enums;

namespace GameBacklog.Domain.Entities
{
    public class GameStatusHistory
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public GameStatus PreviousStatus { get; set; }
        public GameStatus NewStatus { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Comment { get; set; }
    }
}
