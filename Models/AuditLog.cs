namespace SWD392_PROJECT.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }

    public int OrderId { get; set; }

    public int StaffId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public DateTime ActionTime { get; set; }
}
