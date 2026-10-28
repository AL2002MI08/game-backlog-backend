using System;
using System.ComponentModel.DataAnnotations;
using GameBacklog.Domain.Enums;

namespace GameBacklog.Api.Dtos {
    public record GameStatusHistoryResponse {
        [Required]
        public required string GameId { get; init; }

        [Required]
        public required GameStatus PreviousStatus { get; init; }

        [Required]
        public required GameStatus NewStatus { get; init; }

        [Required]
        public required string ChangedBy { get; init; }

        [Required]
        public required DateTime ChangedAt { get; init; }

        public string? Comment { get; init; }
    }
}