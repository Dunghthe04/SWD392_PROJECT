using SWD392_PROJECT.Data;
using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Services;

/// <summary>
/// Service interface for issue reporting operations  
/// Handles business logic for UC11 - Report Issue
/// </summary>
public interface IReportIssueService
{
    /// <summary>
    /// Submit a new issue for an order
    /// </summary>
    Task<IssueSubmitResult> SubmitIssueAsync(int orderId, string details, int studentId);

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

/// <summary>
/// Result object for issue submission
/// </summary>
public class IssueSubmitResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? IssueId { get; set; }
    public Issue? Issue { get; set; }
}

/// <summary>
/// Result object for image upload
/// </summary>
public class ImageUploadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}

/// <summary>
/// Result object for issue status update
/// </summary>
public class IssueUpdateResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Issue? Issue { get; set; }
}

/// <summary>
/// Service implementation for issue reporting operations
/// </summary>
public class ReportIssueService : IReportIssueService
{
    private readonly IIssueRepository _issueRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IValidationService _validationService;

    public ReportIssueService(
        IIssueRepository issueRepository,
        INotificationRepository notificationRepository,
        IOrderRepository orderRepository,
        IValidationService validationService)
    {
        _issueRepository = issueRepository;
        _notificationRepository = notificationRepository;
        _orderRepository = orderRepository;
        _validationService = validationService;
    }

    public async Task<IssueSubmitResult> SubmitIssueAsync(int orderId, string details, int studentId)
    {
        // Validate inputs
        if (orderId <= 0)
        {
            return new IssueSubmitResult
            {
                Success = false,
                Message = "Invalid order ID"
            };
        }

        if (string.IsNullOrWhiteSpace(details))
        {
            return new IssueSubmitResult
            {
                Success = false,
                Message = "Issue details are required"
            };
        }

        // Get order from repository
        var order = _orderRepository.ReadOrder(orderId);
        if (order == null)
        {
            return new IssueSubmitResult
            {
                Success = false,
                Message = "Order not found"
            };
        }

        // Verify order belongs to student
        if (order.StudentId != studentId)
        {
            return new IssueSubmitResult
            {
                Success = false,
                Message = "You can only report issues on your own orders"
            };
        }

        // Check order status - must be completed
        if (order.Status != "Completed")
        {
            return new IssueSubmitResult
            {
                Success = false,
                Message = "Can only report issues on completed orders"
            };
        }

        // Validate 24-hour reporting window
        var timeSinceCompletion = DateTime.UtcNow - order.LastUpdatedAt;
        if (timeSinceCompletion.TotalHours > 24)
        {
            return new IssueSubmitResult
            {
                Success = false,
                Message = "Issue reporting window (24 hours from completion) has expired"
            };
        }

        try
        {
            // Create issue
            var issue = Issue.CreateIssue(orderId, details);
            var createdIssue = await _issueRepository.CreateAsync(issue);

            // Create notification for manager
            var notification = Notification.CreateNotification(
                createdIssue.IssueId,
                $"New issue reported: Order #{orderId} - {details}"
            );
            await _notificationRepository.CreateAsync(notification);

            return new IssueSubmitResult
            {
                Success = true,
                Message = "Issue submitted successfully. Manager will review it shortly.",
                IssueId = createdIssue.IssueId,
                Issue = createdIssue
            };
        }
        catch (Exception ex)
        {
            return new IssueSubmitResult
            {
                Success = false,
                Message = $"Error submitting issue: {ex.Message}"
            };
        }
    }

    public async Task<ImageUploadResult> UploadEvidenceAsync(int issueId, string imagePath)
    {
        // Validate issue exists
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
        {
            return new ImageUploadResult
            {
                Success = false,
                Message = "Issue not found"
            };
        }

        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return new ImageUploadResult
            {
                Success = false,
                Message = "Image path cannot be empty"
            };
        }

        // Validate file format
        var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(imagePath).ToLower();

        if (!validExtensions.Contains(extension))
        {
            return new ImageUploadResult
            {
                Success = false,
                Message = "Only JPG and PNG formats are supported"
            };
        }

        try
        {
            // Update issue with image
            issue.UpdateIssue(newImagePath: imagePath);
            await _issueRepository.UpdateAsync(issue);

            return new ImageUploadResult
            {
                Success = true,
                Message = "Image uploaded successfully",
                ImagePath = imagePath
            };
        }
        catch (Exception ex)
        {
            return new ImageUploadResult
            {
                Success = false,
                Message = $"Error uploading image: {ex.Message}"
            };
        }
    }

    public async Task<List<Issue>> GetAllIssuesAsync()
    {
        return await _issueRepository.GetAllAsync();
    }

    public async Task<List<Issue>> GetIssuesByStatusAsync(string status)
    {
        return await _issueRepository.GetByStatusAsync(status);
    }

    public async Task<List<Issue>> GetIssuesByOrderAsync(int orderId)
    {
        return await _issueRepository.GetByOrderIdAsync(orderId);
    }

    public async Task<Issue?> GetIssueByIdAsync(int issueId)
    {
        return await _issueRepository.GetByIdAsync(issueId);
    }

    public async Task<IssueUpdateResult> UpdateIssueStatusAsync(int issueId, string newStatus)
    {
        // Validate status
        var validStatuses = new[] { "Open", "In Progress", "Resolved", "Closed" };
        if (!validStatuses.Contains(newStatus))
        {
            return new IssueUpdateResult
            {
                Success = false,
                Message = "Invalid status value"
            };
        }

        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
        {
            return new IssueUpdateResult
            {
                Success = false,
                Message = "Issue not found"
            };
        }

        try
        {
            issue.UpdateIssue(newStatus);
            await _issueRepository.UpdateAsync(issue);

            return new IssueUpdateResult
            {
                Success = true,
                Message = $"Issue status updated to {newStatus}",
                Issue = issue
            };
        }
        catch (Exception ex)
        {
            return new IssueUpdateResult
            {
                Success = false,
                Message = $"Error updating issue: {ex.Message}"
            };
        }
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync()
    {
        return await _notificationRepository.GetUnreadAsync();
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
    {
        return await _notificationRepository.MarkAsReadAsync(notificationId);
    }
}
