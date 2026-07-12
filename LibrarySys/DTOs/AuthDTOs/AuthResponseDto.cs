namespace LibrarySys.DTOs.AuthDTOs
{
    public class AuthResponseDto
    {
        public int UserID { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public int? MemberID { get; set; }

        public bool IsActive { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}