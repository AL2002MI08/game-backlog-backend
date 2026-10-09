namespace GameBacklog.Domain.Entities
{
    public class Objective
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public required string Label { get; set; }
        public bool Completed { get; set; }
        public int Position { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}