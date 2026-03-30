using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record FoodStockResponse(
    Guid Id,
    Guid CatId,
    string FoodBrand,
    FoodType FoodType,
    decimal QuantityGrams,
    decimal DailyUsageGrams,
    int LowStockThresholdDays,
    DateTime UpdatedAt,
    decimal DaysRemaining,
    bool IsLowStock
);
