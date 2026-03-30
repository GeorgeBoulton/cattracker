using CatTracker.Domain.Enums;

namespace CatTracker.Domain.Entities;

public class LitterLog
{
    public Guid Id { get; private set; }
    public Guid CatId { get; private set; }
    public DateTime LoggedAt { get; private set; }
    public LitterEntryType EntryType { get; private set; }
    public string? Notes { get; private set; }

    // Private parameterless constructor for EF Core
    private LitterLog() { }

    public static LitterLog Create(
        Guid catId,
        LitterEntryType entryType,
        string? notes = null,
        DateTime? loggedAt = null)
    {
        return new LitterLog
        {
            Id = Guid.NewGuid(),
            CatId = catId,
            LoggedAt = loggedAt ?? DateTime.UtcNow,
            EntryType = entryType,
            Notes = notes,
        };
    }
}
