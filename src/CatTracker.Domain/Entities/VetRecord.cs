using CatTracker.Domain.Enums;

namespace CatTracker.Domain.Entities;

public class VetRecord
{
    public Guid Id { get; private set; }
    public Guid CatId { get; private set; }
    public VetRecordType RecordType { get; private set; }
    public DateOnly Date { get; private set; }
    public string? ClinicName { get; private set; }
    public string? VetName { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public decimal? Cost { get; private set; }
    public DateOnly? NextDueDate { get; private set; }

    // Private parameterless constructor for EF Core
    private VetRecord() { }

    public static VetRecord Create(
        Guid catId,
        VetRecordType recordType,
        DateOnly date,
        string description,
        string? clinicName = null,
        string? vetName = null,
        string? notes = null,
        decimal? cost = null,
        DateOnly? nextDueDate = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));

        if (clinicName is not null && clinicName.Length > 200)
            throw new ArgumentException("ClinicName cannot exceed 200 characters.", nameof(clinicName));

        if (vetName is not null && vetName.Length > 200)
            throw new ArgumentException("VetName cannot exceed 200 characters.", nameof(vetName));

        return new VetRecord
        {
            Id = Guid.NewGuid(),
            CatId = catId,
            RecordType = recordType,
            Date = date,
            Description = description,
            ClinicName = clinicName,
            VetName = vetName,
            Notes = notes,
            Cost = cost,
            NextDueDate = nextDueDate,
        };
    }

    public void Update(
        VetRecordType recordType,
        DateOnly date,
        string description,
        string? clinicName = null,
        string? vetName = null,
        string? notes = null,
        decimal? cost = null,
        DateOnly? nextDueDate = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));

        if (clinicName is not null && clinicName.Length > 200)
            throw new ArgumentException("ClinicName cannot exceed 200 characters.", nameof(clinicName));

        if (vetName is not null && vetName.Length > 200)
            throw new ArgumentException("VetName cannot exceed 200 characters.", nameof(vetName));

        RecordType = recordType;
        Date = date;
        Description = description;
        ClinicName = clinicName;
        VetName = vetName;
        Notes = notes;
        Cost = cost;
        NextDueDate = nextDueDate;
    }
}
