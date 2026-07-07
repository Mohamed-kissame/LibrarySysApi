using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using Microsoft.Data.SqlClient;
using System.Net.Http.Headers;

namespace DAL
{
    public class MemberDAL
    {

        private readonly string _connectionString;

        public MemberDAL(string connectionstring)
        {
            _connectionString = connectionstring;
        }


        public async Task<List<Member>> GetAllMemeberAsync()
        {

            List<Member> Members = new List<Member>();

            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("Sp_Members_GetAll", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;


                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();


                while(await reader.ReadAsync()) { 

                    Members.Add(MapToMember(reader));
                }



            }
            catch(Exception ex)
            {
                throw new Exception("Error retrieving members from the database.", ex);
            }

            return Members;

        }

        public async Task<bool> EmailExistsAsync(string email)
        {

            bool IsExist = false;

            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("sp_Members_EmailExists", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("Email", System.Data.SqlDbType.NVarChar, 100).Value = email;

                SqlParameter outputParam = new SqlParameter("@IsExisting", System.Data.SqlDbType.Bit)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(outputParam);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                IsExist = (bool)outputParam.Value;


            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if email {email} exists in the database.", ex);
            }

            return IsExist;

        }

        public async Task<Member?> GetMemberByIdAsync(int memberId)
        {


            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("dbo.sp_Members_GetById", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@MemberID", System.Data.SqlDbType.Int).Value = memberId;

                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();


                if(await reader.ReadAsync())
                {
                    return MapToMember(reader);
                }


                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving member with ID {memberId} from the database.", ex);
            }
        }

        public async Task<Member?> AddNewMemberAsync(string FullName, string Email, string Phone)
        {


            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("sp_Members_Add", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@FullName", System.Data.SqlDbType.NVarChar, 100).Value = FullName;
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 100).Value = Email;
                command.Parameters.Add("@Phone", System.Data.SqlDbType.NVarChar, 30).Value = Phone;

                SqlParameter NewMemberId = new SqlParameter("@NewMemberID", System.Data.SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(NewMemberId);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                int newMemberId = (int)NewMemberId.Value;

                if(newMemberId > 0)
                {
                    return await GetMemberByIdAsync(newMemberId);
                }

                return null;



            }
            catch(Exception ex)
            {
                throw new Exception($"Error adding new member to the database.", ex);
            }


        }

        public async Task<Member?> UpdateMemberAsync(int memberId, string FullName, string Email, string Phone)
        {


            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("sp_Members_Update", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@MemberID", System.Data.SqlDbType.Int).Value = memberId;
                command.Parameters.Add("@FullName", System.Data.SqlDbType.NVarChar, 100).Value = FullName;
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 100).Value = Email;
                command.Parameters.Add("@Phone", System.Data.SqlDbType.NVarChar, 30).Value = Phone;

                SqlParameter outputParam = new SqlParameter("@IsUpdated", System.Data.SqlDbType.Bit)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(outputParam);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                bool isUpdated = (bool)outputParam.Value;

                if(isUpdated)
                {
                    return await GetMemberByIdAsync(memberId);
                }

                return null;


            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating member with ID {memberId} in the database.", ex);

            }
        }

        public async Task<int> DeleteMemberAsync(int memberId)
        {

            

            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);

                await using SqlCommand command = new SqlCommand("sp_Members_Delete", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@MemberID", System.Data.SqlDbType.Int).Value = memberId;

                SqlParameter resultcodeParam = new SqlParameter("@ResultCode", System.Data.SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(resultcodeParam);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                int ResultCode = (int)resultcodeParam.Value;

                return ResultCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting member with ID {memberId} from the database.", ex);


            }

           
        }

        public async Task<bool> MemberExistsAsync(int memberId)
        {
            bool exists = false;
            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("dbo.sp_Member_ExistsById", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add("@MemberID", System.Data.SqlDbType.Int).Value = memberId;

                SqlParameter outputParam = new SqlParameter("@Exists", System.Data.SqlDbType.Bit)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(outputParam);

                await connection.OpenAsync();

                
                await command.ExecuteNonQueryAsync();

                exists = (bool)outputParam.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if member with ID {memberId} exists in the database.", ex);
            }
            return exists;
        }


        public async Task<int> GetTotalMembersAsync()
        {

            int TotalMembers = 0;

            try
            {

                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("dbo.Sp_GetTotalMembers", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                SqlParameter TotalMembersOutput = new SqlParameter("@TotalMembers", System.Data.SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(TotalMembersOutput);

                await connection.OpenAsync();

                await command.ExecuteScalarAsync();

                TotalMembers = (int)TotalMembersOutput.Value;


                return TotalMembers;



            }catch(Exception ex)
            {
                throw new Exception("Error while get the Total of Members" , ex);
            }

        }

        private static Member MapToMember(SqlDataReader reader)
        {
            return new Member
            {
                MemberID = reader.GetInt32(reader.GetOrdinal("MemberID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

    }
}
