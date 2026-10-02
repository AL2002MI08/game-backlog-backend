using System.ComponentModel.DataAnnotations;

namespace GameBacklog.Api.Dtos {
    public record RegisterRequest {
        [Required]
        [EmailAddress]
        public required string Email {get; init;}

        [Required]
        [MinLength(8, ErrorMessage="Password must be at least 8 characters long")]
        public required string Password {get; init;}
    }
}
