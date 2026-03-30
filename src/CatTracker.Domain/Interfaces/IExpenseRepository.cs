using CatTracker.Domain.Entities;

namespace CatTracker.Domain.Interfaces;

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> GetByDateRangeAsync(Guid catId, DateOnly from, DateOnly to);
    Task<Expense?> GetByIdAsync(Guid id);
    Task AddAsync(Expense expense);
    Task DeleteAsync(Guid id);
}
