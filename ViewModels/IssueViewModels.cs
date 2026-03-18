namespace SWD392_PROJECT.ViewModels;

/// <summary>
/// ViewModel for submitting a new issue report
/// </summary>
public class ReportIssueViewModel
{
    public int OrderId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string OrderDescription { get; set; } = string.Empty;
    public string IssueDetails { get; set; } = string.Empty;
    public IFormFile? EvidenceImage { get; set; }
}

/// <summary>
/// ViewModel for displaying issue details
/// </summary>
public class IssueDetailViewModel
{
    public int IssueId { get; set; }
    public int OrderId { get; set; }
    public string Details { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public List<NotificationViewModel> Notifications { get; set; } = new();
}

/// <summary>
/// ViewModel for listing issues
/// </summary>
public class IssueListViewModel
{
    public List<IssueListItemViewModel> Issues { get; set; } = new();
    public int TotalCount { get; set; }
    public string? FilterStatus { get; set; }
}

/// <summary>
/// ViewModel for issue list item
/// </summary>
public class IssueListItemViewModel
{
    public int IssueId { get; set; }
    public int OrderId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int NotificationCount { get; set; }
}

/// <summary>
/// ViewModel for notifications
/// </summary>
public class NotificationViewModel
{
    public int NotificationId { get; set; }
    public int IssueId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// ViewModel for updating issue status
/// </summary>
public class UpdateIssueStatusViewModel
{
    public int IssueId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public List<string> AvailableStatuses { get; set; } = new() { "Open", "In Progress", "Resolved", "Closed" };
}
