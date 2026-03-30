using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;
using CatTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatTracker.Infrastructure.Repositories;

public class FoodStockRepository : IFoodStockRepository
{
    private readonly CatTrackerDbContext _context;

    public FoodStockRepository(CatTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FoodStock>> GetByCatAsync(Guid catId)
    {
        return await _context.FoodStocks
            .Where(f => f.CatId == catId)
            .OrderBy(f => f.FoodBrand)
            .ToListAsync();
    }

    public async Task<FoodStock?> GetByIdAsync(Guid id)
    {
        return await _context.FoodStocks.FindAsync(id);
    }

    public async Task AddAsync(FoodStock stock)
    {
        await _context.FoodStocks.AddAsync(stock);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FoodStock stock)
    {
        _context.FoodStocks.Update(stock);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var stock = await _context.FoodStocks.FindAsync(id);
        if (stock is not null)
        {
            _context.FoodStocks.Remove(stock);
            await _context.SaveChangesAsync();
        }
    }
}
