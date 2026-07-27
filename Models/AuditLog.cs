namespace Models
{
    public class AuditLog
    {
        public long AuditLogID { get; set; }
        public int? UserID { get; set; }

        public string EventType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;

        public string? EntityName { get; set; }
        public int? EntityID { get; set; }

        public string Result { get; set; } = string.Empty;
        public string? Reason { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public string? RequestPath { get; set; }
        public string? HttpMethod { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}