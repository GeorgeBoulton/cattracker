using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;
using CatTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatTracker.Infrastructure.Repositories;

public class FeedingLogRepository : IFeedingLogRepository
{
    private readonly CatTrackerDbContext _context;

    public FeedingLogRepository(CatTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FeedingLog>> GetRecentAsync(Guid catId, int count = 20)
    {
        return await _context.FeedingLogs
            .Where(f => f.CatId == catId)
            .OrderByDescending(f => f.LoggedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<FeedingLog?> GetLatestAsync(Guid catId)
    {
        return await _context.FeedingLogs
            .Where(f => f.CatId == catId)
            .OrderByDescending(f => f.LoggedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<string>> GetDistinctBrandsAsync(Guid catId)
    {
        return await _context.FeedingLogs
            .Where(f => f.CatId == catId)
            .Select(f => f.FoodBrand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();
    }

    public async Task AddAsync(FeedingLog log)
    {
        await _context.FeedingLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var log = await _context.FeedingLogs.FindAsync(id);
        if (log is not null)
        {
            _context.FeedingLogs.Remove(log);
            await _context.SaveChangesAsync();
        }
    }
}
