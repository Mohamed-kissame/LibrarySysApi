using System.ComponentModel;
using BLL;
using LibrarySys.DTOs.BorrowingDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySys.Controllers
{
    [Authorize]
    [Route("api/borrowings")]
    [ApiController]
    public class BorrowingController : ControllerBase
    {
        private readonly BorrowingService _borrowingService;

        public BorrowingController(BorrowingService borrowingService)
        {
            _borrowingService = borrowingService;
        }

        private static BorrowingResponseDto MapBorrowingToResponseDTO(Borrowing borrowing)
        {
            return new BorrowingResponseDto
            {
                BorrowingID = borrowing.BorrowingID,
                BookID = borrowing.BookID,
                BookTitle = borrowing.BookTitle,
                MemberID = borrowing.MemberID,
                MemberName = borrowing.MemberName,
                BorrowDate = borrowing.BorrowDate,
                DueDate = borrowing.DueDate,
                ReturnDate = borrowing.ReturnDate,
                Status = borrowing.Status,
                IsLate = borrowing.Status.Equals("Borrowed", StringComparison.OrdinalIgnoreCase) && borrowing.DueDate < DateTime.Now,
                CreatedAt = borrowing.CreatedAt,
                UpdatedAt = borrowing.UpdatedAt
            };
        }



        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BorrowingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<BorrowingResponseDto>>> GetAllBorrowingAsync()
        {

            try
            {
                List<Borrowing> borrowings = await _borrowingService.GetAllBorrowingsAsync();

                var response = borrowings.Select(MapBorrowingToResponseDTO).ToList();

                return Ok(response);


            } catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,"An unexpected error occurred. Please try again later.");
            }


        }

        [HttpGet("{borrowingID:int}")]
        [ProducesResponseType(typeof(BorrowingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<BorrowingResponseDto>> GetBorrowingByIDAsync(int borrowingID)
        {
            if (borrowingID <= 0)
            {
                return BadRequest("Invalid Borrowing ID.");
            }
            try
            {
                var borrowing = await _borrowingService.GetBorrowingByIDAsync(borrowingID);
                if (borrowing == null)
                {
                    return NotFound($"Borrowing with ID {borrowingID} not found.");
                }
                var response = MapBorrowingToResponseDTO(borrowing);
                return Ok(response);
            }
            catch (Exception )
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("book/{bookID:int}")]
        [ProducesResponseType(typeof(IEnumerable<BorrowingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<BorrowingResponseDto>>> GetBorrowingByBookIDAsync(int bookID)
        {
            if (bookID <= 0)
            {
                return BadRequest("Invalid Book ID.");
            }
            try
            {
                List<Borrowing> borrowings =
                    await _borrowingService.GetBorrowingByBookIDAsync(bookID);

                var response = borrowings.Select(MapBorrowingToResponseDTO).ToList();

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("member/{memberID:int}")]
        [ProducesResponseType(typeof(IEnumerable<BorrowingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<BorrowingResponseDto>>> GetBorrowingByMemberIDAsync(int memberID)
        {
            if (memberID <= 0)
            {
                return BadRequest("Invalid Member ID.");
            }
            try
            {
                List<Borrowing> borrowings = await _borrowingService.GetBorrowingByMemberIDAsync(memberID);

                var response = borrowings.Select(MapBorrowingToResponseDTO).ToList();

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(BorrowingResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<BorrowingResponseDto>> AddNewBorrowingAsync([FromBody] CreateBorrowingDto createBorrowingDto)
        {

            if (createBorrowingDto == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                var newBorrowing = await _borrowingService.AddBorrowingAsync(createBorrowingDto.BookID, createBorrowingDto.MemberID);

                var response = MapBorrowingToResponseDTO(newBorrowing);

              return CreatedAtAction(nameof(GetBorrowingByIDAsync), new { borrowingID = response.BorrowingID }, response);

            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");


            }

        }


        [HttpPut("{borrowingID:int}/return")]
        [ProducesResponseType(typeof(BorrowingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<BorrowingResponseDto>> ReturnBorrowingAsync(int borrowingID)
        {
            if (borrowingID <= 0)
            {
                return BadRequest("Invalid Borrowing ID.");
            }
            try
            {
                var updatedBorrowing = await _borrowingService.ReturnBorrowingAsync(borrowingID);
                var response = MapBorrowingToResponseDTO(updatedBorrowing);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("TotalBorrowing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetTotalBorrowingAsync()
        {


            try
            {

                var response = await _borrowingService.GetTotalBorrowingsAsync();
                return Ok(response);


            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }

        }

    }
}