using Microsoft.Data.SqlClient;
using Models;
using System.Data;

namespace DAL
{
    public class AuditLogDAL
    {
        private readonly string _connectionString;

        public AuditLogDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<(int ResultCode, long NewAuditLogID)> AddAuditLogAsync(AuditLog auditLog)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            using SqlCommand command = new SqlCommand("dbo.sp_AuditLogs_Add", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserID", SqlDbType.Int).Value = auditLog.UserID.HasValue ? auditLog.UserID.Value : DBNull.Value;

            command.Parameters.Add("@EventType", SqlDbType.NVarChar, 50).Value = auditLog.EventType;

            command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = auditLog.Action;

            command.Parameters.Add("@EntityName", SqlDbType.NVarChar, 50).Value = ToDbValue(auditLog.EntityName);

            command.Parameters.Add("@EntityID", SqlDbType.Int).Value = auditLog.EntityID.HasValue ? auditLog.EntityID.Value : DBNull.Value;

            command.Parameters.Add("@Result", SqlDbType.NVarChar, 20).Value =  auditLog.Result;

            command.Parameters.Add("@Reason", SqlDbType.NVarChar, 200).Value = ToDbValue(auditLog.Reason);

            command.Parameters.Add("@IpAddress", SqlDbType.NVarChar, 45).Value = ToDbValue(auditLog.IpAddress);

            command.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 300).Value = ToDbValue(auditLog.UserAgent);

            command.Parameters.Add("@RequestPath", SqlDbType.NVarChar, 200).Value = ToDbValue(auditLog.RequestPath);

            command.Parameters.Add("@HttpMethod", SqlDbType.NVarChar, 10).Value = ToDbValue(auditLog.HttpMethod);

            SqlParameter newAuditLogIDParam = new SqlParameter("@NewAuditLogID", SqlDbType.BigInt)
            {
                Direction = ParameterDirection.Output
            };

            SqlParameter resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(newAuditLogIDParam);
            command.Parameters.Add(resultCodeParam);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            int resultCode = Convert.ToInt32(resultCodeParam.Value);
            long newAuditLogID = Convert.ToInt64(newAuditLogIDParam.Value);

            return (resultCode, newAuditLogID);
        }

        private static object ToDbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }
    }
}