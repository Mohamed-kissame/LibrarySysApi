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

        public string AccessToken { get; set; } = string.Empty;

        public DateTime AccessTokenExpiresAt { get; set; }

        public string RefreshToken { get;set; } = string.Empty; 
    }
}
