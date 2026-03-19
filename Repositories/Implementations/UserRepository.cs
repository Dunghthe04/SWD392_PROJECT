using SWD392_PROJECT.Data;
using SWD392_PROJECT.Models;
using SWD392_PROJECT.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SWD392_PROJECT.Repositories.Implementations;

/// <summary>
/// Data access implementation for User entity
/// Provides database operations for User model
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> AddAsync(User user)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> IsEmailAvailableAsync(string email, int excludeUserId = 0)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (existingUser == null)
            return true;

        return existingUser.UserId == excludeUserId;
    }
}
