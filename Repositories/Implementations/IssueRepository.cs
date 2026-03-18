using SWD392_PROJECT.Data.Repositories.Interfaces;
using SWD392_PROJECT.Models;
using Microsoft.EntityFrameworkCore;

namespace SWD392_PROJECT.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Issue entity using EF Core
/// </summary>
public class IssueRepository : IIssueRepository
{
    private readonly AppDbContext _context;

    public IssueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Issue>> GetAllAsync()
    {
        return await _context.Issues
            .Include(i => i.Order)
            .Include(i => i.Notifications)
            .ToListAsync();
    }

    public async Task<Issue?> GetByIdAsync(int issueId)
    {
        return await _context.Issues
            .Include(i => i.Order)
            .Include(i => i.Notifications)
            .FirstOrDefaultAsync(i => i.IssueId == issueId);
    }

    public async Task<List<Issue>> GetByOrderIdAsync(int orderId)
    {
        return await _context.Issues
            .Where(i => i.OrderId == orderId)
            .Include(i => i.Notifications)
            .ToListAsync();
    }

    public async Task<List<Issue>> GetByStatusAsync(string status)
    {
        return await _context.Issues
            .Where(i => i.Status == status)
            .Include(i => i.Order)
            .Include(i => i.Notifications)
            .ToListAsync();
    }

    public async Task<Issue> CreateAsync(Issue issue)
    {
        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();
        return issue;
    }

    public async Task<Issue> UpdateAsync(Issue issue)
    {
        _context.Issues.Update(issue);
        await _context.SaveChangesAsync();
        return issue;
    }

    public async Task<bool> DeleteAsync(int issueId)
    {
        var issue = await GetByIdAsync(issueId);
        if (issue == null)
            return false;

        _context.Issues.Remove(issue);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int issueId)
    {
        return await _context.Issues.AnyAsync(i => i.IssueId == issueId);
    }
}
