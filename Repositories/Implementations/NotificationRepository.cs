using SWD392_PROJECT.Data.Repositories.Interfaces;
using SWD392_PROJECT.Models;
using Microsoft.EntityFrameworkCore;

namespace SWD392_PROJECT.Data.Repositories.Implementations;

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
