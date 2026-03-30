namespace CatTracker.Domain.Entities;

public class WaterLog
{
    public Guid Id { get; private set; }
    public Guid CatId { get; private set; }
    public DateTime CleanedAt { get; private set; }
    public string? Notes { get; private set; }

    // Private parameterless constructor for EF Core
    private WaterLog() { }

    public static WaterLog Create(
        Guid catId,
        string? notes = null,
        DateTime? cleanedAt = null)
    {
        if (notes is not null && notes.Length > 500)
            throw new ArgumentException("Notes cannot exceed 500 characters.", nameof(notes));

        return new WaterLog
        {
            Id = Guid.NewGuid(),
            CatId = catId,
            CleanedAt = cleanedAt ?? DateTime.UtcNow,
            Notes = notes,
        };
    }
}
