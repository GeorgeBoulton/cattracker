using CatTracker.Application.DTOs;

namespace CatTracker.Application.Interfaces;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseResponse>> GetByDateRangeAsync(Guid catId, DateOnly from, DateOnly to);
    Task<ExpenseResponse?> GetByIdAsync(Guid id);
    Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request);
    Task DeleteAsync(Guid id);
    Task<IReadOnlyList<MonthlyExpenseSummary>> GetMonthlyTotalsAsync(Guid catId, DateOnly from, DateOnly to);
}
