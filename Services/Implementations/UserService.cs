using SWD392_PROJECT.Data;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace SWD392_PROJECT.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message)> RegisterUserAsync(string username, string email, string password, string phone, string role)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return (false, "Username must be at least 3 characters long.");

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            return (false, "Please provide a valid email address.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters long.");

        // Validate role
        var validRoles = new[] { "Student", "CanteenStaff", "Manager" };
        if (!validRoles.Contains(role))
            return (false, "Invalid role selected.");

        // Check if username already exists
        if (await GetUserByUsernameAsync(username) != null)
            return (false, "Username already exists. Please choose a different one.");

        // Check if email already exists
        if (await GetUserByEmailAsync(email) != null)
            return (false, "Email already registered. Please use a different one or try logging in.");

        try
        {
            var passwordHash = HashPassword(password);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                Phone = phone,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Registration successful! Please log in.");
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred during registration: {ex.Message}");
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, string newEmail, string newPhone)
    {
        if (!IsValidEmailFormat(newEmail))
            return (false, "E1: Invalid Data Format - Please check your email.");

        if (!IsValidPhoneFormat(newPhone))
            return (false, "E1: Invalid Data Format - Phone number must be valid.");

        var currentUser = await GetUserByIdAsync(userId);
        if (currentUser == null)
            return (false, "E2: User not found.");

        if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != currentUser.Email)
        {
            var existingEmail = await GetUserByEmailAsync(newEmail);
            if (existingEmail != null)
                return (false, "E1: This email is already in use.");
        }

        try
        {
            currentUser.SaveChanges(newEmail, newPhone);
            _context.Users.Update(currentUser);
            await _context.SaveChangesAsync();
            return (true, "Profile Updated Successfully");
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    private string HashPassword(string password)
    {
        // Using bcrypt for secure password hashing
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhoneFormat(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        phone = phone.Trim();
        if (phone.Length < 10 || phone.Length > 15)
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?[0-9\-\s\(\)]+$");
    }
}
