namespace SWD392_PROJECT.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }

    public int OrderId { get; set; }

    public int StaffId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public DateTime ActionTime { get; set; }

    public static AuditLog CreateAuditLog(int targetOrderId, string actionName, int actorStaffId, string actor = "CanteenStaff")
    {
        return new AuditLog
        {
            OrderId = targetOrderId,
            ActionType = $"{actionName} by {actor}",
            StaffId = actorStaffId,
            ActionTime = DateTime.UtcNow
        };
    }

    public static AuditLog CreateAuditLog(int targetOrderId, string actionName)
    {
        return CreateAuditLog(targetOrderId, actionName, 0);
    }

    public string ReadAuditLog()
    {
        return $"Order ID: {OrderId}, Action: {ActionType}, Date: {ActionTime:O}";
    }

    public void UpdateAuditLog()
    {
        // Audit log should not be modified after creation.
    }

    public void DeleteAuditLog()
    {
        // Audit log should not be deleted.
    }
}
