namespace SWD392_PROJECT.Services;

public interface INotificationService
{
    void NotifyOrderUpdated(int studentId, int orderId);
}
