namespace CatTracker.Application.DTOs;

public record UpdateCatRequest(
    string Name,
    string? Breed,
    DateOnly? DateOfBirth,
    string? PhotoUrl
);
