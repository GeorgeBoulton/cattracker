using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record LogFeedingRequest(
    Guid CatId,
    string FoodBrand,
    FoodType FoodType,
    decimal AmountGrams,
    string? Notes,
    DateTime? LoggedAt
);
