namespace SWD392_PROJECT.Models;

/// <summary>
/// Represents a user account in the Campus Food system
/// </summary>
public class User
{
    /// <summary>
    /// Primary key - unique user identifier
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Unique username for login
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password hash (using bcrypt or similar)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Path to user's avatar image
    /// </summary>
    public string? AvatarPath { get; set; }

    /// <summary>
    /// User's role (Student, CanteenStaff, Manager)
    /// </summary>
    public string Role { get; set; } = "Student";

    /// <summary>
    /// User creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Is account active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Updates user's email and phone information
    /// </summary>
    /// <param name="newEmail">New email address (null or empty will be ignored)</param>
    /// <param name="newPhone">New phone number (null or empty will be ignored)</param>
    public void SaveChanges(string? newEmail, string? newPhone)
    {
        if (!string.IsNullOrWhiteSpace(newEmail))
        {
            Email = newEmail;
        }

        if (!string.IsNullOrWhiteSpace(newPhone))
        {
            Phone = newPhone;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates user's avatar path
    /// </summary>
    /// <param name="newAvatarPath">New avatar path</param>
    public void UpdateAvatarPath(string? newAvatarPath)
    {
        if (!string.IsNullOrWhiteSpace(newAvatarPath))
        {
            AvatarPath = newAvatarPath;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
