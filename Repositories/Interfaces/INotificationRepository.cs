using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Notification entity - handles notification data access
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Get all notifications
    /// </summary>
    Task<List<Notification>> GetAllAsync();

    /// <summary>
    /// Get notification by ID
    /// </summary>
    Task<Notification?> GetByIdAsync(int notificationId);

    /// <summary>
    /// Get all notifications for a specific issue
    /// </summary>
    Task<List<Notification>> GetByIssueIdAsync(int issueId);

    /// <summary>
    /// Get unread notifications
    /// </summary>
    Task<List<Notification>> GetUnreadAsync();

    /// <summary>
    /// Create a new notification
    /// </summary>
    Task<Notification> CreateAsync(Notification notification);

    /// <summary>
    /// Update notification
    /// </summary>
    Task<Notification> UpdateAsync(Notification notification);

    /// <summary>
    /// Delete notification
    /// </summary>
    Task<bool> DeleteAsync(int notificationId);

    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task<bool> MarkAsReadAsync(int notificationId);

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    Task<bool> MarkAllAsReadAsync();
}
