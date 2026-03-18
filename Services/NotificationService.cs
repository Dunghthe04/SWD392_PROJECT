namespace SWD392_PROJECT.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public void NotifyOrderUpdated(int studentId, int orderId)
    {
        _logger.LogInformation("Notification sent to StudentId={StudentId} for OrderId={OrderId}", studentId, orderId);
    }
}
