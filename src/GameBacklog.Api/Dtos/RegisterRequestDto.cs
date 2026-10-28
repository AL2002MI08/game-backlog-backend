using System.ComponentModel.DataAnnotations;

namespace GameBacklog.Api.Dtos {
    public record RegisterRequest {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public required string Email {get; init;}

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [MaxLength(128)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W_]).+$", ErrorMessage = "Password must contain at least one uppercase letter and a special character")]
        public required string Password { get; init; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public required string ConfirmPassword { get; init; }
    }
}
