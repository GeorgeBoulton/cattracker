using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;
using CatTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatTracker.Infrastructure.Repositories;

public class CatRepository : ICatRepository
{
    private readonly CatTrackerDbContext _context;

    public CatRepository(CatTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<Cat?> GetByIdAsync(Guid id)
    {
        return await _context.Cats.FindAsync(id);
    }

    public async Task<IReadOnlyList<Cat>> GetAllActiveAsync()
    {
        return await _context.Cats
            .Where(c => c.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(Cat cat)
    {
        await _context.Cats.AddAsync(cat);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cat cat)
    {
        _context.Cats.Update(cat);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var cat = await _context.Cats.FindAsync(id);
        if (cat is not null)
        {
            _context.Cats.Remove(cat);
            await _context.SaveChangesAsync();
        }
    }
}
