using System.ComponentModel.DataAnnotations;

namespace LibrarySys.DTOs.AuthDTOs
{
    public class RegisterUserDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Admin|Librarian|Member)$",
            ErrorMessage = "Role must be Admin, Librarian, or Member.")]
        public string Role { get; set; } = string.Empty;

        public int? MemberID { get; set; }
    }
}