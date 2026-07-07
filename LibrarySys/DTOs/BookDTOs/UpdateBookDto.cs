using System.ComponentModel.DataAnnotations;

namespace LibrarySys.DTOs.BookDTOs
{
    public class UpdateBookDto
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


    }
}
