using System.ComponentModel.DataAnnotations;

namespace LibrarySys.DTOs.AuthDTOs
{
    public class RegisterUserDto
    {

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password {  get; set; } = string.Empty;

        public string Role {  get; set; } = string.Empty;

        public int? MemberID { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }



    }
}
