using System.Security.Claims;
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
        private readonly AuditLogService _auditLogService;

        public BooksController(BookService bookService , AuditLogService auditLogService)
        {
            _bookService = bookService;
            _auditLogService = auditLogService;
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

                if (createBookDto == null)
                {
                    await _auditLogService.TryAddAuditLogAsync(
                        CreateAuditLog(
                            action: "CreateBook",
                            result: "Failed",
                            reason: "Request body cannot be null.",
                            entityName: "Books"
                        )
                    );

                    return BadRequest("Request body cannot be null.");
                }


                if (!ModelState.IsValid)
                {
                    await _auditLogService.TryAddAuditLogAsync(
                        CreateAuditLog(
                            action: "CreateBook",
                            result: "Failed",
                            reason: "Invalid book data.",
                            entityName: "Books"
                        )
                    );

                    return BadRequest("Invalid book data. Please ensure all required fields are provided and valid.");
                }



                Book book = await _bookService.AddNewBookAsync(
                    createBookDto.Title,
                    createBookDto.Author,
                    createBookDto.ISBN,
                    createBookDto.TotalCopies
                );

                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "CreateBook",
                        result: "Success",
                        reason: "Book created successfully.",
                        entityName: "Books",
                        entityID: book.BookID
                    )
                );

                var response = MapToBookResponseDto(book);

                return CreatedAtAction(
                    nameof(GetBookByIdAsync),
                    new { bookId = book.BookID },
                    response
                );
            }
            catch (ArgumentException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "CreateBook",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Books"
                    )
                );

                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "CreateBook",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Books"
                    )
                );

                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "CreateBook",
                        result: "Failed",
                        reason: "Unexpected error while creating book.",
                        entityName: "Books"
                    )
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later."
                );
            }
        }

        [HttpPut("{bookId}")]
        [Authorize(Roles = "Admin,Librarian")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBook(int bookId, [FromBody] UpdateBookDto updateBookDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await _auditLogService.TryAddAuditLogAsync(
                           CreateAuditLog(
                                action: "UpdateBook",
                                result: "Failed",
                                reason: "Invalid book data.",
                                entityName: "Books",
                                entityID: bookId
                           )
                    );

                    return BadRequest(ModelState);
                }

                await _bookService.UpdateBookAsync(bookId, updateBookDto.Title , updateBookDto.Author , updateBookDto.ISBN);

                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "UpdateBook",
                        result: "Success",
                        reason: "Book updated successfully.",
                        entityName: "Books",
                        entityID: bookId
                    )
                );

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "UpdateBook",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Books",
                        entityID: bookId
                    )
                );

                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "UpdateBook",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Books",
                        entityID: bookId
                    )
                );

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "UpdateBook",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Books",
                        entityID: bookId
                    )
                );

                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "UpdateBook",
                        result: "Failed",
                        reason: "Unexpected error while updating book.",
                        entityName: "Books",
                        entityID: bookId
                    )
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while updating the book." }
                );
            }
        }


        [HttpDelete("{bookId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            try
            {
                await _bookService.DeleteBookAsync(bookId);

                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "DeleteBook",
                        result: "Success",
                        reason: "Book deleted successfully.",
                        entityName: "Books",
                        entityID: bookId
                    )
                );

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "DeleteBook",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Books",
                        entityID: bookId
                    )
                );

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "DeleteBook",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Books",
                        entityID: bookId
                    )
                );

                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "DeleteBook",
                        result: "Failed",
                        reason: "Unexpected error while deleting book.",
                        entityName: "Books",
                        entityID: bookId
                    )
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while deleting the book." }
                );
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

        private int? GetCurrentUserID()
        {
            string? userIDValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIDValue, out int userID))
            {
                return userID;
            }

            return null;
        }

        private string? GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return Request.Headers["User-Agent"].ToString();
        }

        private AuditLog CreateAuditLog(
            string action,
            string result,
            string? reason = null,
            string? entityName = null,
            int? entityID = null)
        {
            return new AuditLog
            {
                UserID = GetCurrentUserID(),
                EventType = "Audit",
                Action = action,
                EntityName = entityName,
                EntityID = entityID,
                Result = result,
                Reason = reason,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                RequestPath = HttpContext.Request.Path.ToString(),
                HttpMethod = HttpContext.Request.Method
            };
        }
    }
}