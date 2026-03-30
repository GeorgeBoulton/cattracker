using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record CreateFoodStockRequest(
    Guid CatId,
    string FoodBrand,
    FoodType FoodType,
    decimal QuantityGrams,
    decimal DailyUsageGrams,
    int LowStockThresholdDays = 7
);
