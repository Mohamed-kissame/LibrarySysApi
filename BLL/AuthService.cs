using System;
using System.Threading.Tasks;
using DAL;
using Models;

namespace BLL
{
    public class AuthService
    {
        private readonly UserDAL _userDAL;

        public AuthService(UserDAL userDAL)
        {
            _userDAL = userDAL;
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

            if (!IsValidEmail(email)) throw new ArgumentException("Invalid full name. Full name must be non-empty and up to 100 characters.");

            if(!IsValidPassword(password)) throw new ArgumentException("Invalid password. Password must be between 8 and 30 characters.");

          

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
    }
}