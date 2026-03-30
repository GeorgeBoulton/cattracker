using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record LitterLogResponse(
    Guid Id,
    Guid CatId,
    DateTime LoggedAt,
    LitterEntryType EntryType,
    string? Notes
);
