using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;
using CatTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatTracker.Infrastructure.Repositories;

public class WaterLogRepository : IWaterLogRepository
{
    private readonly CatTrackerDbContext _context;

    public WaterLogRepository(CatTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WaterLog>> GetRecentAsync(Guid catId, int count = 20)
    {
        return await _context.WaterLogs
            .Where(w => w.CatId == catId)
            .OrderByDescending(w => w.CleanedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<WaterLog?> GetLatestAsync(Guid catId)
    {
        return await _context.WaterLogs
            .Where(w => w.CatId == catId)
            .OrderByDescending(w => w.CleanedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(WaterLog log)
    {
        await _context.WaterLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var log = await _context.WaterLogs.FindAsync(id);
        if (log is not null)
        {
            _context.WaterLogs.Remove(log);
            await _context.SaveChangesAsync();
        }
    }
}
