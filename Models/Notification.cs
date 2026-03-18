namespace SWD392_PROJECT.Models;

public class Notification
{
    public int NotificationId { get; set; }

    public int IssueId { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedDate { get; set; }

    // Navigation property
    public Issue? Issue { get; set; }

    /// <summary>
    /// Creates a new notification for an issue
    /// </summary>
    public static Notification CreateNotification(int issueId, string message)
    {
        return new Notification
        {
            IssueId = issueId,
            Message = message,
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Reads notification information
    /// </summary>
    public string ReadNotification()
    {
        return $"Notification ID: {NotificationId}, Issue ID: {IssueId}, Message: {Message}, Read: {IsRead}, Created: {CreatedDate:O}";
    }

    /// <summary>
    /// Marks notification as read
    /// </summary>
    public void MarkAsRead()
    {
        IsRead = true;
    }

    /// <summary>
    /// Marks notification as unread
    /// </summary>
    public void MarkAsUnread()
    {
        IsRead = false;
    }
}
