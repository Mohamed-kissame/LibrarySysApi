using Microsoft.Data.SqlClient;
using Models;
using System.Data;

namespace DAL
{
    public class RefreshTokenDAL
    {
        private readonly string _connectionString;

        public RefreshTokenDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<(int ResultCode, int NewRefreshTokenID)> AddRefreshTokenAsync(int userID, string tokenHash, DateTime expiresAt)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            using SqlCommand command = new SqlCommand("dbo.sp_RefreshTokens_Add", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
            command.Parameters.Add("@TokenHash", SqlDbType.NVarChar, 255).Value = tokenHash;
            command.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = expiresAt;

            SqlParameter newRefreshTokenIDParam = new SqlParameter("@NewRefreshTokenID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            SqlParameter resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(newRefreshTokenIDParam);
            command.Parameters.Add(resultCodeParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            int resultCode = Convert.ToInt32(resultCodeParam.Value);
            int newRefreshTokenID = Convert.ToInt32(newRefreshTokenIDParam.Value);

            return (resultCode, newRefreshTokenID);
        }

        public async Task<RefreshToken?> GetRefreshTokenByIDAsync(int refreshTokenID)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            using SqlCommand command = new SqlCommand("dbo.sp_RefreshTokens_GetByID", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@RefreshTokenID", SqlDbType.Int).Value = refreshTokenID;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapRefreshToken(reader);
            }

            return null;
        }

        public async Task<int> RevokeRefreshTokenAsync( int refreshTokenID, string reasonRevoked)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            using SqlCommand command = new SqlCommand("dbo.sp_RefreshTokens_Revoke", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@RefreshTokenID", SqlDbType.Int).Value = refreshTokenID;
            command.Parameters.Add("@ReasonRevoked", SqlDbType.NVarChar, 100).Value =
                string.IsNullOrWhiteSpace(reasonRevoked)
                    ? "Logout"
                    : reasonRevoked;

            SqlParameter resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(resultCodeParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return Convert.ToInt32(resultCodeParam.Value);
        }

        public async Task<(int ResultCode, int NewRefreshTokenID)> RotateRefreshTokenAsync( int oldRefreshTokenID, string newTokenHash, DateTime newExpiresAt)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            using SqlCommand command = new SqlCommand("dbo.sp_RefreshTokens_Rotate", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@OldRefreshTokenID", SqlDbType.Int).Value = oldRefreshTokenID;
            command.Parameters.Add("@NewTokenHash", SqlDbType.NVarChar, 255).Value = newTokenHash;
            command.Parameters.Add("@NewExpiresAt", SqlDbType.DateTime2).Value = newExpiresAt;

            SqlParameter newRefreshTokenIDParam = new SqlParameter("@NewRefreshTokenID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            SqlParameter resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(newRefreshTokenIDParam);
            command.Parameters.Add(resultCodeParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            int resultCode = Convert.ToInt32(resultCodeParam.Value);
            int newRefreshTokenID = Convert.ToInt32(newRefreshTokenIDParam.Value);

            return (resultCode, newRefreshTokenID);
        }

        private static RefreshToken MapRefreshToken(SqlDataReader reader)
        {
            return new RefreshToken
            {
                RefreshTokenID = Convert.ToInt32(reader["RefreshTokenID"]),
                UserID = Convert.ToInt32(reader["UserID"]),
                TokenHash = Convert.ToString(reader["TokenHash"]) ?? string.Empty,
                ExpiresAt = Convert.ToDateTime(reader["ExpiresAt"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),

                RevokedAt = reader["RevokedAt"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["RevokedAt"]),

                ReplacedByRefreshTokenID = reader["ReplacedByRefreshTokenID"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["ReplacedByRefreshTokenID"]),

                ReasonRevoked = reader["ReasonRevoked"] == DBNull.Value
                    ? null
                    : Convert.ToString(reader["ReasonRevoked"])
            };
        }
    }
}