using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Domain.Interfaces;
using CatTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatTracker.Infrastructure.Repositories;

public class LitterLogRepository : ILitterLogRepository
{
    private readonly CatTrackerDbContext _context;

    public LitterLogRepository(CatTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LitterLog>> GetSinceAsync(Guid catId, DateTime since)
    {
        return await _context.LitterLogs
            .Where(l => l.CatId == catId && l.LoggedAt >= since)
            .OrderByDescending(l => l.LoggedAt)
            .ToListAsync();
    }

    public async Task<LitterLog?> GetLatestAsync(Guid catId)
    {
        return await _context.LitterLogs
            .Where(l => l.CatId == catId)
            .OrderByDescending(l => l.LoggedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<LitterLog?> GetLatestFullChangeAsync(Guid catId)
    {
        return await _context.LitterLogs
            .Where(l => l.CatId == catId && l.EntryType == LitterEntryType.FullChange)
            .OrderByDescending(l => l.LoggedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(LitterLog log)
    {
        await _context.LitterLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var log = await _context.LitterLogs.FindAsync(id);
        if (log is not null)
        {
            _context.LitterLogs.Remove(log);
            await _context.SaveChangesAsync();
        }
    }
}
