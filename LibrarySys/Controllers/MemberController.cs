using System.Net;
using System.Security.Claims;
using BLL;
using LibrarySys.DTOs.MemberDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace LibrarySys.Controllers
{
   
    [Route("api/members")]
    [ApiController]
    public class MemberController : ControllerBase
    {

        private readonly MemberService _memberService;
        private readonly AuditLogService _auditLogService;


        public MemberController(MemberService memberService, AuditLogService auditLogService)
        {
            _memberService = memberService;
            _auditLogService = auditLogService;
        }

        private static ResponseMemberDTO MapToResponseMemberDTO(Member member)
        {
            return new ResponseMemberDTO
            {
                MemberID = member.MemberID,
                FullName = member.FullName,
                Email = member.Email,
                Phone = member.Phone,
                IsActive = member.IsActive,
                CreatedAt = member.CreatedAt,
                UpdatedAt = member.UpdatedAt
            };
        }


        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ResponseMemberDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<ResponseMemberDTO>>> GetAllMemebersAsync()
        {

            try
            {

                List<Member> members = await _memberService.GetAllMembersAsync();

                var response = members.Select(MapToResponseMemberDTO).ToList();

                return Ok(response);


            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }


        }


        [Authorize(Policy = "CanAccessMemberData")]
        [HttpGet("{memberID:int}")]
        [ProducesResponseType(typeof(ResponseMemberDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ResponseMemberDTO>> GetMemberByIDAsync(int memberID)
        {
            if (memberID <= 0)
            {
                return BadRequest("Invalid member ID. It must be a positive integer.");
            }
            try
            {

                Member? member = await _memberService.GetMemberByIDAsync(memberID);
                if (member == null)
                {
                    return NotFound($"Member with ID {memberID} not found.");
                }


                var response = MapToResponseMemberDTO(member);

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMemberDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseMemberDTO>> AddNewMemberAsync([FromBody] CreateMemberDTO createMemberDTO)
        {
            try
            {
                if (createMemberDTO == null)
                {
                    await AuditAsync(
                        action: "CreateMember",
                        result: "Failed",
                        reason: "Request body cannot be null.",
                        entityName: "Members"
                    );

                    return BadRequest("Request body cannot be null.");
                }

                if (!ModelState.IsValid)
                {
                    await AuditAsync(
                        action: "CreateMember",
                        result: "Failed",
                        reason: "Invalid member data.",
                        entityName: "Members"
                    );

                    return BadRequest(ModelState);
                }

                Member member = await _memberService.AddNewMemberAsync(
                    createMemberDTO.FullName,
                    createMemberDTO.Email,
                    createMemberDTO.Phone
                );

                var response = MapToResponseMemberDTO(member);

                await AuditAsync(
                    action: "CreateMember",
                    result: "Success",
                    reason: "Member created successfully.",
                    entityName: "Members",
                    entityID: response.MemberID
                );

                return CreatedAtAction(
                    nameof(GetMemberByIDAsync),
                    new { memberID = response.MemberID },
                    response
                );
            }
            catch (InvalidOperationException ex)
            {
                await AuditAsync(
                    action: "CreateMember",
                    result: "Failed",
                    reason: ex.Message,
                    entityName: "Members"
                );

                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                await AuditAsync(
                    action: "CreateMember",
                    result: "Failed",
                    reason: ex.Message,
                    entityName: "Members"
                );

                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                await AuditAsync(
                    action: "CreateMember",
                    result: "Failed",
                    reason: "Unexpected error while creating member.",
                    entityName: "Members"
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later."
                );
            }
        }

        [HttpPut("{memberId}")]
        [Authorize(Roles = "Admin,Librarian")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMember(int memberId, [FromBody] UpdateMemberDTO updateMemberDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await AuditAsync(
                        action: "UpdateMember",
                        result: "Failed",
                        reason: "Invalid member data.",
                        entityName: "Members",
                        entityID: memberId
                    );

                    return BadRequest(ModelState);
                }

                await _memberService.UpdateMemberAsync(memberId, updateMemberDto.FullName , updateMemberDto.Email , updateMemberDto.Phone);

                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "UpdateMember",
                        result: "Success",
                        reason: "Member updated successfully.",
                        entityName: "Members",
                        entityID: memberId
                    )
                );

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "UpdateMember",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Members",
                        entityID: memberId
                    )
                );

                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "UpdateMember",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Members",
                        entityID: memberId
                    )
                );

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                await AuditAsync(
                    action: "UpdateMember",
                    result: "Failed",
                    reason: ex.Message,
                    entityName: "Members",
                    entityID: memberId
                );

                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "UpdateMember",
                        result: "Failed",
                        reason: "Unexpected error while updating member.",
                        entityName: "Members",
                        entityID: memberId
                    )
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while updating the member." }
                );
            }
        }


        [HttpDelete("{memberId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMember(int memberId)
        {
            try
            {
                await _memberService.DeleteMemberAsync(memberId);

                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "DeleteMember",
                        result: "Success",
                        reason: "Member deleted successfully.",
                        entityName: "Members",
                        entityID: memberId
                    )
                );

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "DeleteMember",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Members",
                        entityID: memberId
                    )
                );

                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "DeleteMember",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "Members",
                        entityID: memberId
                    )
                );

                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        action: "DeleteMember",
                        result: "Failed",
                        reason: "Unexpected error while deleting member.",
                        entityName: "Members",
                        entityID: memberId
                    )
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while deleting the member." }
                );
            }
        }

        [Authorize(Roles = "Admin,Librarian")]
        [HttpGet("TotalMembers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetCountMembersAsync()
        {

            try
            {

                var response = await _memberService.GetTotalMembersAsync();

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

        private async Task AuditAsync(string action, string result, string reason, string entityName, int? entityID = null)
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