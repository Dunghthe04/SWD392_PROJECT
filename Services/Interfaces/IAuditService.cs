namespace SWD392_PROJECT.Services.Interfaces;

public interface IAuditService
{
    void Log(string actionType, int orderId, int staffId);
}
