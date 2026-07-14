using System.Net;
using BLL;
using LibrarySys.DTOs.MemberDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace LibrarySys.Controllers
{
    [Authorize]
    [Route("api/members")]
    [ApiController]
    public class MemberController : ControllerBase
    {

        private readonly MemberService _memberService;

        public MemberController(MemberService memberService)
        {
            _memberService = memberService;
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


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ResponseMemberDTO>), StatusCodes.Status200OK)]
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


        [HttpGet("{memberID:int}")]
        [ProducesResponseType(typeof(ResponseMemberDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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


        [HttpPost]
        [ProducesResponseType(typeof(ResponseMemberDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ResponseMemberDTO>> AddNewMemberAsync([FromBody] CreateMemberDTO createMemberDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Member member = await _memberService.AddNewMemberAsync(createMemberDTO.FullName, createMemberDTO.Email, createMemberDTO.Phone);

                var response = MapToResponseMemberDTO(member);

                return CreatedAtAction(nameof(GetAllMemebersAsync), new { memberID = response.MemberID }, response);

            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }

        }


        [HttpPut("{memberID:int}")]
        [ProducesResponseType(typeof(ResponseMemberDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ResponseMemberDTO>> UpdateMemberAsync(int memberID, [FromBody] UpdateMemberDTO updateMemberDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (memberID <= 0)
            {
                return BadRequest("Invalid member ID. It must be a positive integer.");
            }
            try
            {
                Member member = await _memberService.UpdateMemberAsync(memberID, updateMemberDTO.FullName, updateMemberDTO.Email, updateMemberDTO.Phone);
                var response = MapToResponseMemberDTO(member);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }

        }


        [HttpDelete("{memberID:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> DeleteMemberAsync(int memberID)
        {
            try
            {
                if (memberID <= 0)
                {
                    return BadRequest("Invalid Member ID. Member ID must be a positive integer.");
                }
                await _memberService.DeleteMemberAsync(memberID);


                return NoContent();
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


        [HttpGet("TotalMembers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
    }
}