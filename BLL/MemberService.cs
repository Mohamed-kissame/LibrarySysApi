using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DAL;
using Models;

namespace BLL
{
    public class MemberService
    {

        private readonly MemberDAL _memberDAL;

        public MemberService(MemberDAL memberDAL)
        {
            _memberDAL = memberDAL;
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _memberDAL.GetAllMemeberAsync();
        }

        public async Task<Member?> GetMemberByIDAsync(int memberID)
        {
            return await _memberDAL.GetMemberByIdAsync(memberID);
        }

        private bool IsValidFullName(string fullName)
        {
            return !string.IsNullOrWhiteSpace(fullName) && fullName.Length <= 100;
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Length <= 100 && email.Contains("@");
        }

        private bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && phone.Length <= 30;
        }

        private async Task<bool> CheckEmailExistAsync(string email)
        {
            return await _memberDAL.EmailExistsAsync(email);
        }

        public async Task<Member> AddNewMemberAsync(string fullName, string email, string phone)
        {
            if (!IsValidFullName(fullName))
            {
                throw new ArgumentException("Invalid full name. Full name must be non-empty and up to 100 characters.");
            }
            if (!IsValidEmail(email))
            {
                throw new ArgumentException("Invalid email. Email must be non-empty, contain '@', and be up to 100 characters.");
            }
            if (!IsValidPhone(phone))
            {
                throw new ArgumentException("Invalid phone. Phone must be non-empty and up to 30 characters.");
            }
            if (await CheckEmailExistAsync(email))
            {
                throw new InvalidOperationException("A member with the same email already exists.");
            }

            Member? member = await _memberDAL.AddNewMemberAsync(fullName, email, phone);

            if (member == null)
            {
                throw new Exception("Failed to add new member.");

            }


            return member;
        }

        public async Task<Member> UpdateMemberAsync(int memberID, string fullName, string email, string phone)
        {
            if (!IsValidFullName(fullName))
            {
                throw new ArgumentException("Invalid full name. Full name must be non-empty and up to 100 characters.");
            }
            if (!IsValidEmail(email))
            {
                throw new ArgumentException("Invalid email. Email must be non-empty, contain '@', and be up to 100 characters.");
            }
            if (!IsValidPhone(phone))
            {
                throw new ArgumentException("Invalid phone. Phone must be non-empty and up to 30 characters.");
            }
            Member? existingMember = await _memberDAL.GetMemberByIdAsync(memberID);

            if (existingMember == null)
            {
                throw new KeyNotFoundException($"Member with ID {memberID} not found.");
            }

            bool isEmailchanged = !string.Equals(existingMember.Email, email, StringComparison.OrdinalIgnoreCase);

            if (isEmailchanged && await CheckEmailExistAsync(email))
            {
                throw new InvalidOperationException("A member with the same email already exists.");
            }

            Member? memeber = await _memberDAL.UpdateMemberAsync(memberID, fullName, email, phone);

            if (memeber == null)
            {
                throw new Exception("Failed to update the Member.");
            }

            return memeber;

        }

        public async Task<bool> DeleteMemberAsync(int memberID)
        {
           

            if(memberID <= 0)
            {
                throw new ArgumentException("Member ID must be greater than zero.", nameof(memberID));
            }

            int Result = await _memberDAL.DeleteMemberAsync(memberID);

            return Result switch
            {
                1 => true,
                -1 => throw new KeyNotFoundException($"Member with ID {memberID} was not found."),
                -2 => throw new InvalidOperationException("Cannot delete the member because it has active borrowings."),
                _ => throw new Exception("Failed to delete the member.")
            };

        }

        public async Task<int> GetTotalMembersAsync()
        {

            int Results = await _memberDAL.GetTotalMembersAsync();

            return Results;

        }
    }
}