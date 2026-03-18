namespace SWD392_PROJECT.Services.Interfaces;

public interface INotificationService
{
    void NotifyOrderUpdated(int studentId, int orderId);
}
