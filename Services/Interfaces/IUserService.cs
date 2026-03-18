using SWD392_PROJECT.Models;

namespace SWD392_PROJECT.Services.Interfaces;

public interface IUserService
{
    Task<(bool Success, string Message)> RegisterUserAsync(string username, string email, string password, string phone, string role);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    bool VerifyPassword(string password, string hash);
    Task<(bool Success, string Message)> UpdateProfileAsync(int userId, string newEmail, string newPhone);
}
