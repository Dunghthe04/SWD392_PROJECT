using SWD392_PROJECT.Models;
using Microsoft.EntityFrameworkCore;

namespace SWD392_PROJECT.Data;

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

/// <summary>
/// Repository implementation for Notification entity using EF Core
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetAllAsync()
    {
        return await _context.Notifications
            .Include(n => n.Issue)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(int notificationId)
    {
        return await _context.Notifications
            .Include(n => n.Issue)
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
    }

    public async Task<List<Notification>> GetByIssueIdAsync(int issueId)
    {
        return await _context.Notifications
            .Where(n => n.IssueId == issueId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetUnreadAsync()
    {
        return await _context.Notifications
            .Where(n => !n.IsRead)
            .Include(n => n.Issue)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> DeleteAsync(int notificationId)
    {
        var notification = await GetByIdAsync(notificationId);
        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        var notification = await GetByIdAsync(notificationId);
        if (notification == null)
            return false;

        notification.MarkAsRead();
        await UpdateAsync(notification);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync()
    {
        var unreadNotifications = await GetUnreadAsync();
        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        if (unreadNotifications.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        return true;
    }
}
