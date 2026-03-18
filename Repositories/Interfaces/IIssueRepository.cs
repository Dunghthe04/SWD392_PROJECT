using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Issue entity - handles data access operations
/// </summary>
public interface IIssueRepository
{
    /// <summary>
    /// Get all issues
    /// </summary>
    Task<List<Issue>> GetAllAsync();

    /// <summary>
    /// Get issue by ID
    /// </summary>
    Task<Issue?> GetByIdAsync(int issueId);

    /// <summary>
    /// Get all issues for a specific order
    /// </summary>
    Task<List<Issue>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Get issues by status
    /// </summary>
    Task<List<Issue>> GetByStatusAsync(string status);

    /// <summary>
    /// Create a new issue
    /// </summary>
    Task<Issue> CreateAsync(Issue issue);

    /// <summary>
    /// Update existing issue
    /// </summary>
    Task<Issue> UpdateAsync(Issue issue);

    /// <summary>
    /// Delete issue by ID
    /// </summary>
    Task<bool> DeleteAsync(int issueId);

    /// <summary>
    /// Check if issue exists
    /// </summary>
    Task<bool> ExistsAsync(int issueId);
}
