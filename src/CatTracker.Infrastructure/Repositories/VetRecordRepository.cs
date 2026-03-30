using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;
using CatTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatTracker.Infrastructure.Repositories;

public class VetRecordRepository : IVetRecordRepository
{
    private readonly CatTrackerDbContext _context;

    public VetRecordRepository(CatTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VetRecord>> GetByCatAsync(Guid catId)
    {
        return await _context.VetRecords
            .Where(v => v.CatId == catId)
            .OrderByDescending(v => v.Date)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<VetRecord>> GetUpcomingAsync(Guid catId, DateOnly withinDays)
    {
        return await _context.VetRecords
            .Where(v => v.CatId == catId && v.NextDueDate != null && v.NextDueDate <= withinDays)
            .OrderBy(v => v.NextDueDate)
            .ToListAsync();
    }

    public async Task<VetRecord?> GetByIdAsync(Guid id)
    {
        return await _context.VetRecords.FindAsync(id);
    }

    public async Task AddAsync(VetRecord record)
    {
        await _context.VetRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(VetRecord record)
    {
        _context.VetRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var record = await _context.VetRecords.FindAsync(id);
        if (record is not null)
        {
            _context.VetRecords.Remove(record);
            await _context.SaveChangesAsync();
        }
    }
}
