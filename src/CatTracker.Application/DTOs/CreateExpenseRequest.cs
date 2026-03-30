using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record CreateExpenseRequest(
    Guid CatId,
    ExpenseCategory Category,
    decimal Amount,
    DateOnly Date,
    string Description,
    string? Notes,
    bool IsRecurring = false,
    int? RecurringIntervalDays = null
);
