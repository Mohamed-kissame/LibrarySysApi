using System.Security.Claims;
using BLL;
using LibrarySys.DTOs.BorrowingDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace LibrarySys.Controllers
{
    
    [Route("api/borrowings")]
    [ApiController]
    public class BorrowingController : ControllerBase
    {
        private readonly BorrowingService _borrowingService;
        private readonly AuditLogService _auditLogService;

        public BorrowingController(BorrowingService borrowingService , AuditLogService auditLogService)
        {
            _borrowingService = borrowingService;
            _auditLogService = auditLogService;
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


        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BorrowingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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


        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet("{borrowingID:int}")]
        [ProducesResponseType(typeof(BorrowingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet("book/{bookID:int}")]
        [ProducesResponseType(typeof(IEnumerable<BorrowingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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


        [Authorize(Policy = "CanAccessMemberData")]
        [HttpGet("member/{memberID:int}")]
        [ProducesResponseType(typeof(IEnumerable<BorrowingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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


        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        [ProducesResponseType(typeof(BorrowingResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BorrowingResponseDto>> AddNewBorrowingAsync([FromBody] CreateBorrowingDto createBorrowingDto)
        {
            try
            {
                if (createBorrowingDto == null)
                {
                    await AuditAsync(
                        action: "BorrowBook",
                        result: "Failed",
                        reason: "Request body cannot be null.",
                        entityName: "Borrowings"
                    );

                    return BadRequest("Request body cannot be null.");
                }

                if (!ModelState.IsValid)
                {
                    await AuditAsync(
                        action: "BorrowBook",
                        result: "Failed",
                        reason: "Invalid borrowing data.",
                        entityName: "Borrowings"
                    );

                    return BadRequest(ModelState);
                }

                var newBorrowing = await _borrowingService.AddBorrowingAsync(
                    createBorrowingDto.BookID,
                    createBorrowingDto.MemberID
                );

                var response = MapBorrowingToResponseDTO(newBorrowing);

                await AuditAsync(
                    action: "BorrowBook",
                    result: "Success",
                    reason: "Book borrowed successfully.",
                    entityName: "Borrowings",
                    entityID: response.BorrowingID
                );

                return CreatedAtAction(
                    nameof(GetBorrowingByIDAsync),
                    new { borrowingID = response.BorrowingID },
                    response
                );
            }
            catch (ArgumentException ex)
            {
                await AuditAsync(
                    action: "BorrowBook",
                    result: "Failed",
                    reason: ex.Message,
                    entityName: "Borrowings"
                );

                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await AuditAsync(
                    action: "BorrowBook",
                    result: "Failed",
                    reason: ex.Message,
                    entityName: "Borrowings"
                );

                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await AuditAsync(
                    action: "BorrowBook",
                    result: "Failed",
                    reason: ex.Message,
                    entityName: "Borrowings"
                );

                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                await AuditAsync(
                    action: "BorrowBook",
                    result: "Failed",
                    reason: "Unexpected error while borrowing book.",
                    entityName: "Borrowings"
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later."
                );
            }
        }


        [Authorize(Roles = "Admin,Librarian")]
        [HttpPut("{borrowingID:int}/return")]
        [ProducesResponseType(typeof(BorrowingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BorrowingResponseDto>> ReturnBorrowingAsync(int borrowingID)
        {
            try
            {
                if (borrowingID <= 0)
                {
                    await AuditAsync(
                        action: "ReturnBook",
                        result: "Failed",
                        reason: "Invalid Borrowing ID.",
                        entityName: "Borrowings",
                        entityID: borrowingID
                    );

                    return BadRequest("Invalid Borrowing ID.");
                }

                var updatedBorrowing = await _borrowingService.ReturnBorrowingAsync(borrowingID);

                var response = MapBorrowingToResponseDTO(updatedBorrowing);

                await AuditAsync(
                    action: "ReturnBook",
                    result: "Success",
                    reason: "Book returned successfully.",
                    entityName: "Borrowings",
                    entityID: borrowingID
                );

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                await AuditAsync(
                    action: "ReturnBook",
                    result: "Failed",
                    reason: ex.Message,
                    entityName: "Borrowings",
                    entityID: borrowingID
                );

                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await AuditAsync(
                    action: "ReturnBook",
                    result: "Failed",
                    reason: ex.Message,
                    entityName: "Borrowings",
                    entityID: borrowingID
                );

                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await AuditAsync(
                    action: "ReturnBook",
                    result: "Failed",
                    reason: ex.Message,
                    entityName: "Borrowings",
                    entityID: borrowingID
                );

                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                await AuditAsync(
                    action: "ReturnBook",
                    result: "Failed",
                    reason: "Unexpected error while returning book.",
                    entityName: "Borrowings",
                    entityID: borrowingID
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later."
                );
            }
        }


        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet("TotalBorrowing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        private async Task AuditAsync( string action, string result,string reason,string entityName,int? entityID = null)
        {
            await _auditLogService.TryAddAuditLogAsync(
                CreateAuditLog(
                    action: action,
                    result: result,
                    reason: reason,
                    entityName: entityName,
                    entityID: entityID
                )
            );
        }
    }
}