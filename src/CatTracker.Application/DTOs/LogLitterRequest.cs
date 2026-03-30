using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record LogLitterRequest(
    Guid CatId,
    LitterEntryType EntryType,
    string? Notes,
    DateTime? LoggedAt
);
