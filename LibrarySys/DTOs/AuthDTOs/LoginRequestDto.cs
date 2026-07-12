using System.ComponentModel.DataAnnotations;

namespace LibrarySys.DTOs.AuthDTOs
{
    public class LoginRequestDto
    {
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}