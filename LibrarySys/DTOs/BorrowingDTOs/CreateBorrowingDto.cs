using System.ComponentModel.DataAnnotations;

namespace LibrarySys.DTOs.BorrowingDTOs
{
    public class CreateBorrowingDto
    {
        [Range(1, int.MaxValue)]
        public int BookID { get; set; }

        [Range(1, int.MaxValue)]
        public int MemberID { get; set; }
    }
}