using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;
using CatTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatTracker.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly CatTrackerDbContext _context;

    public ExpenseRepository(CatTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Expense>> GetByDateRangeAsync(Guid catId, DateOnly from, DateOnly to)
    {
        return await _context.Expenses
            .Where(e => e.CatId == catId && e.Date >= from && e.Date <= to)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<Expense?> GetByIdAsync(Guid id)
    {
        return await _context.Expenses.FindAsync(id);
    }

    public async Task AddAsync(Expense expense)
    {
        await _context.Expenses.AddAsync(expense);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense is not null)
        {
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
        }
    }
}
