using System.ComponentModel.DataAnnotations;

namespace LibrarySys.DTOs.BookDTOs
{
    public class CreateBookDto
    {

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;


        [Required]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;


        [Required]
        [MaxLength(30)]
        public string ISBN { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1.")]
        public int TotalCopies { get; set; }


    }
}
