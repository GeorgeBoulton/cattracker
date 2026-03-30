namespace CatTracker.Application.DTOs;

public record WaterLogResponse(
    Guid Id,
    Guid CatId,
    DateTime CleanedAt,
    string? Notes
);
