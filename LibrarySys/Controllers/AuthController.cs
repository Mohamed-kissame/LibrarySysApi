using System.Security.Claims;
using BLL;
using LibrarySys.DTOs.AuthDTOs;
using LibrarySys.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Models;

namespace LibrarySys.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AuditLogService _auditLogService;

        public AuthController(AuthService authService , JwtTokenService jwtTokenService , AuditLogService auditLogService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
            _auditLogService = auditLogService;
        }

        [HttpPost("register")]
        [EnableRateLimiting("RegisterRateLimit")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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
        [EnableRateLimiting("LoginRateLimit")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                User user = await _authService.LoginAsync(loginDto.Email, loginDto.Password);

                await _auditLogService.TryAddAuditLogAsync( CreateAuditLog (
                eventType: "Authentication",
                action: "LoginSuccess",
                result: "Success",
                reason: "User logged in successfully.",
                userID: user.UserID
                 )
                );

                var tokenResult = _jwtTokenService.GenerateToken(user);

                string refreshToken = await _authService.CreateRefreshTokenAsync(user.UserID);

                AuthResponseDto response = MapUserToAuthResponseDto(user , tokenResult.Token , tokenResult.ExpiresAt , refreshToken);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                await _auditLogService.TryAddAuditLogAsync(
                  CreateAuditLog(
                    eventType: "Authentication",
                    action: "LoginFailed",
                    result: "Failed",
                    reason: "Invalid email/password."
                   )
                );

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


        [HttpPost("refresh")]
        [EnableRateLimiting("RefreshRateLimit")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var refreshResult = await _authService.RotateRefreshTokenAsync(refreshTokenDto.RefreshToken);

                await _auditLogService.TryAddAuditLogAsync(
                          CreateAuditLog(
                             eventType: "Authentication",
                             action: "RefreshSuccess",
                             result: "Success",
                             reason: "Refresh token rotated successfully.",
                             entityName: "RefreshTokens",
                             entityID: TryGetRefreshTokenID(refreshTokenDto.RefreshToken),
                             userID: refreshResult.User.UserID
                          )
                );

                var tokenResult = _jwtTokenService.GenerateToken(refreshResult.User);

                AuthResponseDto response = MapUserToAuthResponseDto(
                    refreshResult.User,
                    tokenResult.Token,
                    tokenResult.ExpiresAt,
                    refreshResult.RefreshToken
                );

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                CreateAuditLog(
                 eventType: "Authentication",
                 action: "RefreshFailed",
                 result: "Failed",
                 reason: ex.Message,
                 entityName: "RefreshTokens",
                 entityID: TryGetRefreshTokenID(refreshTokenDto.RefreshToken)
                )
              );

                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                  CreateAuditLog(
                     eventType: "Authentication",
                     action: "RefreshFailed",
                     result: "Failed",
                     reason: ex.Message,
                     entityName: "RefreshTokens",
                     entityID: TryGetRefreshTokenID(refreshTokenDto.RefreshToken)
                  )
                );

                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                await _auditLogService.TryAddAuditLogAsync(
               CreateAuditLog(
                eventType: "Authentication",
                action: "RefreshFailed",
                result: "Failed",
                reason: "Unexpected error while refreshing token.",
                entityName: "RefreshTokens",
                entityID: TryGetRefreshTokenID(refreshTokenDto.RefreshToken)
                )
               );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while refreshing the token." }
                );
            }
        }


        [HttpPost("logout")]
        [EnableRateLimiting("RefreshRateLimit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto logoutDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int userID = await _authService.RevokeRefreshTokenAsync(logoutDto.RefreshToken);

                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        eventType: "Authentication",
                        action: "LogoutSuccess",
                        result: "Success",
                        reason: "User logged out successfully.",
                        entityName: "RefreshTokens",
                        entityID: TryGetRefreshTokenID(logoutDto.RefreshToken),
                        userID: userID
                    )
                );

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        eventType: "Authentication",
                        action: "LogoutFailed",
                        result: "Failed",
                        reason: ex.Message,
                        entityName: "RefreshTokens",
                        entityID: TryGetRefreshTokenID(logoutDto.RefreshToken)
                    )
                );

                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        eventType: "Authentication",
                        action: "LogoutFailed",
                        result: "Failed",
                        reason: "Invalid refresh token.",
                        entityName: "RefreshTokens",
                        entityID: TryGetRefreshTokenID(logoutDto.RefreshToken)
                    )
                );

                return Unauthorized(new { message = "Invalid refresh token." });
            }
            catch (Exception)
            {
                await _auditLogService.TryAddAuditLogAsync(
                    CreateAuditLog(
                        eventType: "Authentication",
                        action: "LogoutFailed",
                        result: "Failed",
                        reason: "Unexpected error while logging out.",
                        entityName: "RefreshTokens",
                        entityID: TryGetRefreshTokenID(logoutDto.RefreshToken)
                    )
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while logging out." }
                );
            }
        }


        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                UserID = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Email = User.FindFirstValue(ClaimTypes.Email),
                Role = User.FindFirstValue(ClaimTypes.Role),
                MemberID = User.FindFirstValue("memberId"),
                FullName = User.FindFirstValue("fullName")
            });
        }

        private static AuthResponseDto MapUserToAuthResponseDto(User user , string? token = null , DateTime? expiresAt = null , string? RefreshToken = null)
        {
            return new AuthResponseDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                MemberID = user.MemberID,
                IsActive = user.IsActive,
                AccessToken = token ?? string.Empty,
                AccessTokenExpiresAt = expiresAt ?? DateTime.MinValue,
                RefreshToken = RefreshToken ?? string.Empty
                
            };
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

        private AuditLog CreateAuditLog(string eventType, string action, string result, string? reason = null, string? entityName = null, int? entityID = null, int? userID = null)
        {
            return new AuditLog
            {
                UserID = userID ?? GetCurrentUserID(),
                EventType = eventType,
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


        private int? TryGetRefreshTokenID(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            string[] parts = refreshToken.Split('.', 2);

            if (parts.Length != 2)
            {
                return null;
            }

            if (int.TryParse(parts[0], out int refreshTokenID))
            {
                return refreshTokenID;
            }

            return null;
        }

    }
}