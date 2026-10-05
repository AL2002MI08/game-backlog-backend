using System.ComponentModel.DataAnnotations;

namespace GameBacklog.Api.Dtos {
    public record LoginRequest {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public required string Email {get; init;}

        [Required]
        [MaxLength(128)]
        public required string Password {get; init;}
    }
}