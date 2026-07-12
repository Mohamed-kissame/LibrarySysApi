using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Models;

namespace DAL
{
    public class UserDAL
    {
        private readonly string _connectionString;

        public UserDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("dbo.sp_Users_GetByEmail", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email.Trim();

                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapUser(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user with email {email} from the database.", ex);
            }
        }

        public async Task<User?> GetUserByIDAsync(int userID)
        {
            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("dbo.sp_Users_GetById", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                await connection.OpenAsync();

                await using SqlDataReader reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapUser(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user with ID {userID} from the database.", ex);
            }
        }

        public async Task<(int ResultCode, User? User)> AddUserAsync(string fullName, string email, string passwordHash, string role, int? memberID)
        {
            int resultCode = 0;

            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("dbo.sp_Users_Add", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName.Trim();
                command.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email.Trim();
                command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = passwordHash;
                command.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = role.Trim();

                command.Parameters.Add("@MemberID", SqlDbType.Int).Value =
                    memberID.HasValue ? memberID.Value : (object)DBNull.Value;

                SqlParameter newUserIDParam = new SqlParameter("@NewUserID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(newUserIDParam);

                SqlParameter resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(resultCodeParam);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                int newUserID = Convert.ToInt32(newUserIDParam.Value);
                resultCode = Convert.ToInt32(resultCodeParam.Value);

                if (resultCode == 1 && newUserID > 0)
                {
                    User? user = await GetUserByIDAsync(newUserID);
                    return (resultCode, user);
                }

                return (resultCode, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new user to the database.", ex);
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                await using SqlConnection connection = new SqlConnection(_connectionString);
                await using SqlCommand command = new SqlCommand("dbo.sp_Users_EmailExists", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email.Trim();

                SqlParameter outputParam = new SqlParameter("@Exists", SqlDbType.Bit)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(outputParam);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                return outputParam.Value != DBNull.Value && Convert.ToBoolean(outputParam.Value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if email {email} exists in the database.", ex);
            }
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),

                PasswordHash = HasColumn(reader, "PasswordHash") && !reader.IsDBNull(reader.GetOrdinal("PasswordHash")) ? reader.GetString(reader.GetOrdinal("PasswordHash")) : string.Empty,

                Role = reader.GetString(reader.GetOrdinal("Role")),

                MemberID = reader.IsDBNull(reader.GetOrdinal("MemberID")) ? null : reader.GetInt32(reader.GetOrdinal("MemberID")),

                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}