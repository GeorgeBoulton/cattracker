using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record FeedingLogResponse(
    Guid Id,
    Guid CatId,
    DateTime LoggedAt,
    string FoodBrand,
    FoodType FoodType,
    decimal AmountGrams,
    string? Notes
);
