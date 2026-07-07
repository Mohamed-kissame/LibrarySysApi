namespace LibrarySys.DTOs.BorrowingDTOs
{
    public class BorrowingResponseDto
    {
        public int BorrowingID { get; set; }

        public int BookID { get; set; }

        public string BookTitle { get; set; } = string.Empty;

        public int MemberID { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public DateTime BorrowDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool IsLate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}