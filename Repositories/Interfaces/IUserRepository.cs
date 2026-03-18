using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Repositories.Interfaces;

/// <summary>
/// Data access interface for User entity
/// Provides methods to interact with User data in the database
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by user ID
    /// </summary>
    /// <param name="userId">The user ID to search for</param>
    /// <returns>User object if found, null otherwise</returns>
    Task<User?> GetByIdAsync(int userId);

    /// <summary>
    /// Retrieves a user by username
    /// </summary>
    /// <param name="username">The username to search for</param>
    /// <returns>User object if found, null otherwise</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Retrieves a user by email
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <returns>User object if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Updates an existing user in the database
    /// </summary>
    /// <param name="user">The user object with updated information</param>
    /// <returns>True if update successful, false otherwise</returns>
    Task<bool> UpdateAsync(User user);

    /// <summary>
    /// Adds a new user to the database
    /// </summary>
    /// <param name="user">The user object to add</param>
    /// <returns>True if add successful, false otherwise</returns>
    Task<bool> AddAsync(User user);

    /// <summary>
    /// Checks if email is already in use
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for edit scenarios)</param>
    /// <returns>True if email is available, false if already in use</returns>
    Task<bool> IsEmailAvailableAsync(string email, int excludeUserId = 0);
}
