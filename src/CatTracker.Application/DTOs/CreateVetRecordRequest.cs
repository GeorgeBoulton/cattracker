using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record CreateVetRecordRequest(
    Guid CatId,
    VetRecordType RecordType,
    DateOnly Date,
    string Description,
    string? ClinicName,
    string? VetName,
    string? Notes,
    decimal? Cost,
    DateOnly? NextDueDate
);
