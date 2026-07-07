using System.ComponentModel.DataAnnotations;

namespace LibrarySys.DTOs.MemberDTOs
{
    public class CreateMemberDTO
    {

        [Required]
        [StringLength(100)]

        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]

        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]

        public string Phone { get; set; } = string.Empty;
    }
}
