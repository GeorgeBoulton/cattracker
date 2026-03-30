namespace CatTracker.Application.DTOs;

public record CatResponse(
    Guid Id,
    string Name,
    string? Breed,
    DateOnly? DateOfBirth,
    string? PhotoUrl,
    bool IsActive,
    DateTime CreatedAt
);
