using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record VetRecordResponse(
    Guid Id,
    Guid CatId,
    VetRecordType RecordType,
    DateOnly Date,
    string? ClinicName,
    string? VetName,
    string Description,
    string? Notes,
    decimal? Cost,
    DateOnly? NextDueDate
);
