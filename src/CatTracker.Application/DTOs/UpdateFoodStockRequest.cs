namespace CatTracker.Application.DTOs;

public record UpdateFoodStockRequest(
    decimal QuantityGrams,
    decimal DailyUsageGrams,
    int LowStockThresholdDays
);
