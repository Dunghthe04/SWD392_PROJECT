namespace SWD392_PROJECT.Models;

public class Issue
{
    public int IssueId { get; set; }

    public int OrderId { get; set; }

    public string Details { get; set; } = string.Empty;

    public string Status { get; set; } = "Open"; // Open, In Progress, Resolved, Closed

    public string? ImagePath { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    // Navigation property
    public Order? Order { get; set; }

    public List<Notification> Notifications { get; set; } = new();

    /// <summary>
    /// Creates a new issue for an order
    /// </summary>
    public static Issue CreateIssue(int orderId, string details)
    {
        var now = DateTime.UtcNow;
        return new Issue
        {
            OrderId = orderId,
            Details = details,
            Status = "Open",
            CreatedDate = now,
            LastUpdatedDate = now
        };
    }

    /// <summary>
    /// Reads issue information
    /// </summary>
    public string ReadIssue()
    {
        return $"Issue ID: {IssueId}, Order ID: {OrderId}, Status: {Status}, Details: {Details}, Created: {CreatedDate:O}";
    }

    /// <summary>
    /// Updates issue status and image path
    /// </summary>
    public void UpdateIssue(string? newStatus = null, string? newImagePath = null)
    {
        if (!string.IsNullOrEmpty(newStatus))
        {
            Status = newStatus;
        }

        if (!string.IsNullOrEmpty(newImagePath))
        {
            ImagePath = newImagePath;
        }

        LastUpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks issue as deleted by changing status to "Closed"
    /// </summary>
    public void DeleteIssue()
    {
        Status = "Closed";
        LastUpdatedDate = DateTime.UtcNow;
    }
}
