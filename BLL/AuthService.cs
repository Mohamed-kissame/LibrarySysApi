using System;
using System.Threading.Tasks;
using DAL;
using Models;

namespace BLL
{
    public class AuthService
    {
        private readonly UserDAL _userDAL;
        private readonly RefreshTokenDAL _refreshTokenDAL;

        public AuthService(UserDAL userDAL , RefreshTokenDAL refreshTokenDAL)
        {
            _userDAL = userDAL;
            _refreshTokenDAL = refreshTokenDAL;
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Length <= 100 && email.Contains("@");
        }

        private bool IsValidFullName(string fullName)
        {
            return !string.IsNullOrWhiteSpace(fullName) && fullName.Length <= 100;
        }

        private bool IsValidRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role) || role.Length > 20)
                return false;

            return role is "Admin" or "Librarian" or "Member";
        }

        private bool IsValidPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 8 && password.Length <= 30;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }

        private async Task<bool> EmailExistsAsync(string email)
        {
            return await _userDAL.EmailExistsAsync(email);
        }

        private string GenerateRefreshTokenSecret()
        {
            byte[] randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        private string HashRefreshTokenSecret(string Secret)
        {
            return BCrypt.Net.BCrypt.HashPassword(Secret);

        }

        private bool VerifyRefreshTokenSecret(string secret, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(secret, storedHash);
        }

        private string BuildRefreshToken(int refreshTokenID, string secret)
        {
            return $"{refreshTokenID}.{secret}";
        }

        private (int RefreshTokenID, string Secret) ParseRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token is required.");

            string[] parts = refreshToken.Split('.', 2);

            if (parts.Length != 2)
                throw new ArgumentException("Invalid refresh token format.");

            if (!int.TryParse(parts[0], out int refreshTokenID) || refreshTokenID <= 0)
                throw new ArgumentException("Invalid refresh token ID.");

            if (string.IsNullOrWhiteSpace(parts[1]))
                throw new ArgumentException("Invalid refresh token secret.");

            return (refreshTokenID, parts[1]);
        }

        public async Task<User> RegisterAsync(string fullName, string email, string password , string role, int? memberID)
        {
            fullName = fullName?.Trim() ?? string.Empty;
            email = email?.Trim().ToLowerInvariant() ?? string.Empty;
            role = role?.Trim() ?? string.Empty;


            if (!IsValidFullName(fullName))
                throw new ArgumentException("Invalid full name. Full name must be non-empty and up to 100 characters.");

            if (!IsValidEmail(email))
                throw new ArgumentException("Invalid email. Email must be non-empty, valid, and up to 100 characters.");

            if (!IsValidPassword(password))
                throw new ArgumentException("Invalid password. Password must be between 8 and 30 characters.");

            if (!IsValidRole(role))
                throw new ArgumentException("Invalid role. Role must be Admin, Librarian, or Member.");

            if (role == "Member" && memberID == null)
                throw new InvalidOperationException("Member role requires a MemberID.");

            if (memberID.HasValue && memberID.Value <= 0)
                throw new ArgumentException("Member ID must be greater than zero.");

            if ((role == "Admin" || role == "Librarian") && memberID != null)
                throw new InvalidOperationException("Admin and Librarian users cannot have a MemberID.");

            if (await EmailExistsAsync(email))
                throw new InvalidOperationException("A user with the same email already exists.");

            string passwordHash = HashPassword(password);

            var results = await _userDAL.AddUserAsync(fullName,email,passwordHash, role,memberID);

            return results.ResultCode switch
            {
                1 when results.User is not null => results.User,

                1 => throw new Exception("User was created but could not be retrieved."),

                -1 => throw new InvalidOperationException("A user with the same email already exists."),

                -2 => throw new ArgumentException("Role must be Admin, Librarian, or Member."),

                -3 => throw new InvalidOperationException("Member role requires a MemberID."),

                -4 => throw new InvalidOperationException("Admin or Librarian users cannot have a MemberID."),

                -5 => throw new KeyNotFoundException($"Member with ID {memberID} was not found or is inactive."),

                -6 => throw new InvalidOperationException("This member already has a user account."),

                _ => throw new Exception("An unexpected error occurred while adding the user.")
            };
        }


        public async Task<User> LoginAsync(string email, string password)
        {
            email = email?.Trim().ToLowerInvariant() ?? string.Empty;

            if (!IsValidEmail(email))
                throw new ArgumentException("Invalid email. Email must be non-empty, valid, and up to 100 characters.");

            if (!IsValidPassword(password)) throw new ArgumentException("Invalid password. Password must be between 8 and 30 characters.");

          

            User? user = await _userDAL.GetUserByEmailAsync(email);

            if(user == null)
            {
                throw new UnauthorizedAccessException("invalid email/password");

            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid email/password");
            }

            if (!VerifyPassword(password, user.PasswordHash)) throw new UnauthorizedAccessException("Invalid email/password");

           
           return user;

        }

        public async Task<string> CreateRefreshTokenAsync(int userID)
        {
            if (userID <= 0)
            {
                throw new ArgumentException("Invalid user ID.");
            }

            string secret = GenerateRefreshTokenSecret();

            string tokenHash = HashRefreshTokenSecret(secret);

            DateTime expiresAt = DateTime.UtcNow.AddDays(7);

            var result = await _refreshTokenDAL.AddRefreshTokenAsync(userID,tokenHash,expiresAt);

            return result.ResultCode switch
            {
                1 when result.NewRefreshTokenID > 0 =>
                    BuildRefreshToken(result.NewRefreshTokenID, secret),

                1 =>
                    throw new Exception("Refresh token was created but no valid ID was returned."),

                -1 =>
                    throw new InvalidOperationException("User was not found or is inactive."),

                -2 =>
                    throw new ArgumentException("Invalid refresh token data."),

                -99 =>
                    throw new Exception("An unexpected database error occurred while creating refresh token."),

                _ =>
                    throw new Exception("An unexpected error occurred while creating refresh token.")
            };
        }

        public async Task<(User User, string RefreshToken)> RotateRefreshTokenAsync(string refreshToken)
        {
            var parsedToken = ParseRefreshToken(refreshToken);

            RefreshToken? storedRefreshToken =
                await _refreshTokenDAL.GetRefreshTokenByIDAsync(parsedToken.RefreshTokenID);

            if (storedRefreshToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            if (storedRefreshToken.RevokedAt != null)
            {
                throw new UnauthorizedAccessException("Refresh token has been revoked.");
            }

            if (storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token has expired.");
            }

            if (!VerifyRefreshTokenSecret(parsedToken.Secret, storedRefreshToken.TokenHash))
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            User? user = await _userDAL.GetUserByIDAsync(storedRefreshToken.UserID);

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User is not active or no longer exists.");
            }

            string newSecret = GenerateRefreshTokenSecret();
            string newTokenHash = HashRefreshTokenSecret(newSecret);
            DateTime newExpiresAt = DateTime.UtcNow.AddDays(7);

            var rotateResult = await _refreshTokenDAL.RotateRefreshTokenAsync(
                storedRefreshToken.RefreshTokenID,
                newTokenHash,
                newExpiresAt
            );

            return rotateResult.ResultCode switch
            {
                1 when rotateResult.NewRefreshTokenID > 0 =>
                    (user, BuildRefreshToken(rotateResult.NewRefreshTokenID, newSecret)),

                1 =>
                    throw new Exception("Refresh token was rotated but no valid new ID was returned."),

                -1 =>
                    throw new UnauthorizedAccessException("Refresh token was not found."),

                -2 =>
                    throw new UnauthorizedAccessException("Refresh token has already been revoked."),

                -3 =>
                    throw new UnauthorizedAccessException("Refresh token has expired."),

                -4 =>
                    throw new UnauthorizedAccessException("User is not active or no longer exists."),

                -5 =>
                    throw new ArgumentException("Invalid refresh token data."),

                -99 =>
                    throw new Exception("An unexpected database error occurred while rotating refresh token."),

                _ =>
                    throw new Exception("An unexpected error occurred while rotating refresh token.")
            };
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var parsedToken = ParseRefreshToken(refreshToken);

            RefreshToken? storedRefreshToken =
                await _refreshTokenDAL.GetRefreshTokenByIDAsync(parsedToken.RefreshTokenID);

            if (storedRefreshToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            if (storedRefreshToken.RevokedAt != null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            if (storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token has expired.");
            }

            if (!VerifyRefreshTokenSecret(parsedToken.Secret, storedRefreshToken.TokenHash))
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            int resultCode = await _refreshTokenDAL.RevokeRefreshTokenAsync(
                storedRefreshToken.RefreshTokenID,
                "Logout"
            );

            switch (resultCode)
            {
                case 1:
                    return;

                case -1:
                    throw new UnauthorizedAccessException("Invalid refresh token.");

                case -2:
                    throw new UnauthorizedAccessException("Invalid refresh token.");

                case -3:
                    throw new ArgumentException("Invalid refresh token data.");

                case -99:
                    throw new Exception("An unexpected database error occurred while revoking refresh token.");

                default:
                    throw new Exception("An unexpected error occurred while revoking refresh token.");
            }
        }
    }
}