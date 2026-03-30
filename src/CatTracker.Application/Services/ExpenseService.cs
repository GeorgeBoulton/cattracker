using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;

    public ExpenseService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<IReadOnlyList<ExpenseResponse>> GetByDateRangeAsync(Guid catId, DateOnly from, DateOnly to)
    {
        var expenses = await _expenseRepository.GetByDateRangeAsync(catId, from, to);

        return expenses
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ExpenseResponse?> GetByIdAsync(Guid id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense is null)
            return null;

        return MapToResponse(expense);
    }

    public async Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request)
    {
        var expense = Expense.Create(
            request.CatId,
            request.Category,
            request.Amount,
            request.Date,
            request.Description,
            request.Notes,
            request.IsRecurring,
            request.RecurringIntervalDays);

        await _expenseRepository.AddAsync(expense);

        return MapToResponse(expense);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _expenseRepository.DeleteAsync(id);
    }

    public async Task<IReadOnlyList<MonthlyExpenseSummary>> GetMonthlyTotalsAsync(Guid catId, DateOnly from, DateOnly to)
    {
        var expenses = await _expenseRepository.GetByDateRangeAsync(catId, from, to);

        return expenses
            .GroupBy(e => new { e.Date.Year, e.Date.Month, e.Category })
            .Select(g => new MonthlyExpenseSummary(g.Key.Year, g.Key.Month, g.Key.Category, g.Sum(e => e.Amount)))
            .OrderBy(s => s.Year)
            .ThenBy(s => s.Month)
            .ThenBy(s => s.Category)
            .ToList();
    }

    private static ExpenseResponse MapToResponse(Expense expense)
    {
        return new ExpenseResponse(
            expense.Id,
            expense.CatId,
            expense.Category,
            expense.Amount,
            expense.Date,
            expense.Description,
            expense.Notes,
            expense.IsRecurring,
            expense.RecurringIntervalDays);
    }
}
