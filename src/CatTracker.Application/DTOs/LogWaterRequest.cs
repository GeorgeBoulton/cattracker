namespace CatTracker.Application.DTOs;

public record LogWaterRequest(
    Guid CatId,
    string? Notes,
    DateTime? CleanedAt
);
