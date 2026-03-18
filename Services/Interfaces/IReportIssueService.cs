using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Services.Interfaces;

/// <summary>
/// Service interface for issue reporting operations  
/// Handles business logic for UC11 - Report Issue
/// </summary>
public interface IReportIssueService
{
    /// <summary>
    /// Submit a new issue for an order
    /// </summary>
    Task<IssueSubmitResult> SubmitIssueAsync(int orderId, string details);

    /// <summary>
    /// Upload evidence image for an issue
    /// </summary>
    Task<ImageUploadResult> UploadEvidenceAsync(int issueId, string imagePath);

    /// <summary>
    /// Get all issues
    /// </summary>
    Task<List<Issue>> GetAllIssuesAsync();

    /// <summary>
    /// Get issues by status
    /// </summary>
    Task<List<Issue>> GetIssuesByStatusAsync(string status);

    /// <summary>
    /// Get issues for a specific order
    /// </summary>
    Task<List<Issue>> GetIssuesByOrderAsync(int orderId);

    /// <summary>
    /// Get issue by ID
    /// </summary>
    Task<Issue?> GetIssueByIdAsync(int issueId);

    /// <summary>
    /// Update issue status
    /// </summary>
    Task<IssueUpdateResult> UpdateIssueStatusAsync(int issueId, string newStatus);

    /// <summary>
    /// Get unread notifications for managers
    /// </summary>
    Task<List<Notification>> GetUnreadNotificationsAsync();

    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task<bool> MarkNotificationAsReadAsync(int notificationId);
}
