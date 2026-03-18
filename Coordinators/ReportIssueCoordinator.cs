using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Coordinators;

/// <summary>
/// UC11 - Report Issue: StudentCoordinator handles issue reporting from student perspective
/// Control class that encapsulates the business logic for submitting issue reports
/// </summary>
public class ReportIssueCoordinator
{
    /// <summary>
    /// Submit an issue report for a completed order
    /// </summary>
    public static IssueSubmissionResult SubmitIssueRequest(Order order, string issueDetails)
    {
        // Validate preconditions
        if (order == null)
        {
            return new IssueSubmissionResult
            {
                Success = false,
                Message = "Order not found"
            };
        }

        if (order.Status != "Completed")
        {
            return new IssueSubmissionResult
            {
                Success = false,
                Message = "Can only report issues on completed orders"
            };
        }

        // Check 24-hour reporting window
        var timeSinceCompletion = DateTime.UtcNow - order.LastUpdatedAt;
        if (timeSinceCompletion.TotalHours > 24)
        {
            return new IssueSubmissionResult
            {
                Success = false,
                Message = "Issue reporting window (24 hours from completion) has expired"
            };
        }

        if (string.IsNullOrWhiteSpace(issueDetails))
        {
            return new IssueSubmissionResult
            {
                Success = false,
                Message = "Issue details are required"
            };
        }

        // Create issue record
        var issue = Issue.CreateIssue(order.OrderId, issueDetails);

        return new IssueSubmissionResult
        {
            Success = true,
            Message = "Issue submitted successfully",
            IssueId = issue.IssueId,
            Issue = issue
        };
    }

    /// <summary>
    /// Upload evidence image for an issue report
    /// </summary>
    public static ImageUploadResult UploadImageFile(Issue issue, string imagePath)
    {
        // Validate preconditions
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

        // Validate file format (JPG, PNG)
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

        // Update issue with image path
        issue.UpdateIssue(newImagePath: imagePath);

        return new ImageUploadResult
        {
            Success = true,
            Message = "Image uploaded successfully",
            ImagePath = imagePath
        };
    }

    /// <summary>
    /// Create a notification for a manager about the issue
    /// </summary>
    public static Notification CreateManagerNotification(Issue issue, string messagePrefix = "New issue reported")
    {
        var message = $"{messagePrefix}: Order #{issue.OrderId} - {issue.Details}";
        var notification = Notification.CreateNotification(issue.IssueId, message);
        return notification;
    }
}

/// <summary>
/// Result object for issue submission
/// </summary>
public class IssueSubmissionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int IssueId { get; set; }
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
