using System.ComponentModel.DataAnnotations;

namespace LibrarySys.DTOs.AuthDTOs
{
    public class LogoutRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}