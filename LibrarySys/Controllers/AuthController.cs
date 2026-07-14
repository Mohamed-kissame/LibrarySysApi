using BLL;
using LibrarySys.DTOs.AuthDTOs;
using LibrarySys.Services;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace LibrarySys.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(AuthService authService , JwtTokenService jwtTokenService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto registerDto)
        {
            try
            {
                User user = await _authService.RegisterAsync(registerDto.FullName, registerDto.Email, registerDto.Password,  registerDto.Role,registerDto.MemberID
                );

                AuthResponseDto response = MapUserToAuthResponseDto(user);

                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while registering the user." }
                );
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                User user = await _authService.LoginAsync(loginDto.Email, loginDto.Password);

                var tokenResult = _jwtTokenService.GenerateToken(user);

                AuthResponseDto response = MapUserToAuthResponseDto(user , tokenResult.Token , tokenResult.ExpiresAt);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid email/password." });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while logging in." }
                );
            }
        }

        private static AuthResponseDto MapUserToAuthResponseDto(User user , string? token = null , DateTime? expiresAt = null)
        {
            return new AuthResponseDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                MemberID = user.MemberID,
                IsActive = user.IsActive,
                Token = token ?? string.Empty,
                ExpiresAt = expiresAt ?? DateTime.MinValue
            };
        }
    }
}