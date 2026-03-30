namespace CatTracker.Domain.Entities;

public class Cat
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Breed { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? PhotoUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Private parameterless constructor for EF Core
    private Cat() { }

    public static Cat Create(
        string name,
        string? breed = null,
        DateOnly? dateOfBirth = null,
        string? photoUrl = null)
    {
        return new Cat
        {
            Id = Guid.NewGuid(),
            Name = name,
            Breed = breed,
            DateOfBirth = dateOfBirth,
            PhotoUrl = photoUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
