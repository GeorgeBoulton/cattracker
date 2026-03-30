using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record ExpenseResponse(
    Guid Id,
    Guid CatId,
    ExpenseCategory Category,
    decimal Amount,
    DateOnly Date,
    string Description,
    string? Notes,
    bool IsRecurring,
    int? RecurringIntervalDays
);
