using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Services.Interfaces;

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
/// Result object for order update operation
/// </summary>
public class UpdateOrderResult
{
    public bool Success { get; init; }
    public bool IsConflict { get; init; }
    public string Message { get; init; } = string.Empty;
    public Order? CurrentOrder { get; init; }
}

/// <summary>
/// Result object for order list retrieval
/// </summary>
public class ViewOrderResult
{
    public List<Order> OrderList { get; init; } = new();
    public string? Message { get; init; }
    public bool IsError { get; init; }
}
