using BLL;
using LibrarySys.DTOs.BookDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace LibrarySys.Controllers
{
    
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {

        private readonly BookService _bookService;

        public BooksController(BookService bookService)
        {
            _bookService = bookService;
        }

        private static BookResponseDto MapToBookResponseDto(Book book)
        {
            return new BookResponseDto
            {
                BookID = book.BookID,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                IsAvailable = book.AvailableCopies > 0,
                IsActive = book.IsActive,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<BookResponseDto>>> GetAllBooksAsync()
        {

            try
            {

                List<Book> books = await _bookService.GetAllBooksAsync();

                var response = books.Select(MapToBookResponseDto).ToList();


                return Ok(response);



            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }


        }



        [HttpGet("{bookId:int}")]
        [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<BookResponseDto>> GetBookByIdAsync(int bookId)
        {
            try
            {
                if (bookId <= 0)
                {
                    return BadRequest("Invalid book ID. Book ID must be a positive integer.");
                }


                Book? book = await _bookService.GetBookByIdAsync(bookId);

                if (book == null)
                {
                    return NotFound($"Book with ID {bookId} not found.");
                }

               

                return Ok(MapToBookResponseDto(book));

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }

        }


        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<BookResponseDto>> AddNewBookAsync([FromBody] CreateBookDto createBookDto)
        {

            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid book data. Please ensure all required fields are provided and valid.");
                }

                Book book = await _bookService.AddNewBookAsync(createBookDto.Title, createBookDto.Author, createBookDto.ISBN, createBookDto.TotalCopies);

                var response = MapToBookResponseDto(book);

                return CreatedAtAction(nameof(GetBookByIdAsync), new { bookId = book.BookID }, response);

            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

        [Authorize(Roles = "Admin,Librarian")]
        [HttpPut("{bookId:int}")]
        [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<BookResponseDto>> UpdateBookAsync(int bookId, [FromBody] UpdateBookDto updateBookDto)
        {
            try
            {
                if (bookId <= 0)
                {
                    return BadRequest("Invalid book ID. Book ID must be a positive integer.");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid book data. Please ensure all required fields are provided and valid.");
                }
                Book updatedBook = await _bookService.UpdateBookAsync(bookId, updateBookDto.Title, updateBookDto.Author, updateBookDto.ISBN);

                var response = MapToBookResponseDto(updatedBook);

                return Ok(response);

            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(KeyNotFoundException ex)
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{bookId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBookAsync(int bookId)
        {
            try
            {
                if (bookId <= 0)
                {
                    return BadRequest("Invalid book ID. Book ID must be a positive integer.");
                }
                await _bookService.DeleteBookAsync(bookId);

                
                return NoContent();
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,"An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet("TotalBooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetCountBooksAsync()
        {

            try
            {

                var response = await _bookService.GetCountBooksAsync();

                return Ok(response);


            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }

        }
    }
}