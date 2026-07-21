using System.ComponentModel.DataAnnotations;

namespace LibrarySys.DTOs.AuthDTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}