namespace LibrarySys.DTOs.BookDTOs
{
    public class BookResponseDto
    {


        public int BookID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;


        public int TotalCopies { get; set; }

        public int AvailableCopies { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

    }
}
