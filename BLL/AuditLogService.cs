using DAL;
using Models;

namespace BLL
{
    public class AuditLogService
    {
        private readonly AuditLogDAL _auditLogDAL;

        public AuditLogService(AuditLogDAL auditLogDAL)
        {
            _auditLogDAL = auditLogDAL;
        }

        public async Task<bool> TryAddAuditLogAsync(AuditLog auditLog)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(auditLog.EventType))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(auditLog.Action))
                {
                    return false;
                }

                if (auditLog.Result is not "Success" and not "Failed" and not "Denied")
                {
                    return false;
                }

                var result = await _auditLogDAL.AddAuditLogAsync(auditLog);

                return result.ResultCode == 1;
            }
            catch
            {
                return false;
            }
        }
    }
}