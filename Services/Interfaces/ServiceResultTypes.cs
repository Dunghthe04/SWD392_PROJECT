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
