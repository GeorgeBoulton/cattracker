using CatTracker.Application.DTOs;
using CatTracker.Application.Services;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Tests.Services;

[TestFixture]
public class LitterServiceTests
{
    private ILitterLogRepository _litterLogRepository = null!;
    private LitterService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _litterLogRepository = Substitute.For<ILitterLogRepository>();
        _sut = new LitterService(_litterLogRepository);
    }

    // LogLitterAsync must return a response with all fields correctly mapped from the request,
    // confirming the entity is created with the right values and the mapping path is correct.
    [Test]
    public async Task LogLitterAsync_ValidRequest_ReturnsCorrectlyMappedResponse()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var loggedAt = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var request = new LogLitterRequest(catId, LitterEntryType.Use, "Some notes", loggedAt);

        // Act
        var result = await _sut.LogLitterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CatId.Should().Be(catId);
        result.EntryType.Should().Be(LitterEntryType.Use);
        result.Notes.Should().Be("Some notes");
        result.LoggedAt.Should().Be(loggedAt);
        await _litterLogRepository.Received(1).AddAsync(Arg.Any<LitterLog>());
    }

    // LogLitterAsync with a null LoggedAt must produce a response with a LoggedAt close to UtcNow,
    // confirming the entity factory defaults to UtcNow rather than leaving the timestamp unset.
    [Test]
    public async Task LogLitterAsync_WithNullLoggedAt_UsesCurrentTime()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var request = new LogLitterRequest(catId, LitterEntryType.TopUp, null, null);

        // Act
        var result = await _sut.LogLitterAsync(request);

        // Assert
        result.LoggedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        await _litterLogRepository.Received(1).AddAsync(Arg.Any<LitterLog>());
    }

    // GetSinceAsync must return all logs returned by the repository, mapped to responses,
    // confirming list mapping works correctly and preserves all entries.
    [Test]
    public async Task GetSinceAsync_WithLogs_ReturnsMappedList()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var since = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var logs = new List<LitterLog>
        {
            LitterLog.Create(catId, LitterEntryType.Use, "First", new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc)),
            LitterLog.Create(catId, LitterEntryType.TopUp, "Second", new DateTime(2025, 6, 1, 18, 0, 0, DateTimeKind.Utc)),
        };
        _litterLogRepository.GetSinceAsync(catId, since).Returns(logs);

        // Act
        var result = await _sut.GetSinceAsync(catId, since);

        // Assert
        result.Should().HaveCount(2);
        result[0].Notes.Should().Be("First");
        result[0].EntryType.Should().Be(LitterEntryType.Use);
        result[1].Notes.Should().Be("Second");
        result[1].EntryType.Should().Be(LitterEntryType.TopUp);
    }

    // GetSinceAsync must return an empty list when the repository returns no logs,
    // confirming the empty collection path does not throw or return null.
    [Test]
    public async Task GetSinceAsync_NoLogs_ReturnsEmptyList()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var since = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _litterLogRepository.GetSinceAsync(catId, since).Returns(new List<LitterLog>());

        // Act
        var result = await _sut.GetSinceAsync(catId, since);

        // Assert
        result.Should().BeEmpty();
    }

    // GetLatestAsync must return the log mapped to a response when the repository returns one,
    // confirming the mapping path for a non-null repository result works correctly.
    [Test]
    public async Task GetLatestAsync_WithLog_ReturnsMappedResponse()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var log = LitterLog.Create(catId, LitterEntryType.Use, "Latest entry", new DateTime(2025, 6, 1, 18, 0, 0, DateTimeKind.Utc));
        _litterLogRepository.GetLatestAsync(catId).Returns(log);

        // Act
        var result = await _sut.GetLatestAsync(catId);

        // Assert
        result.Should().NotBeNull();
        result!.CatId.Should().Be(catId);
        result.EntryType.Should().Be(LitterEntryType.Use);
        result.Notes.Should().Be("Latest entry");
    }

    // GetLatestAsync must return null when the repository returns null,
    // verifying the null propagation path is handled without throwing.
    [Test]
    public async Task GetLatestAsync_NoLog_ReturnsNull()
    {
        // Arrange
        var catId = Guid.NewGuid();
        _litterLogRepository.GetLatestAsync(catId).Returns((LitterLog?)null);

        // Act
        var result = await _sut.GetLatestAsync(catId);

        // Assert
        result.Should().BeNull();
    }

    // GetLatestFullChangeAsync must return the log with EntryType=FullChange when the repository returns one,
    // confirming the mapping path preserves the enum value and is wired to the correct repository method.
    [Test]
    public async Task GetLatestFullChangeAsync_WithFullChangeLog_ReturnsMappedResponse()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var log = LitterLog.Create(catId, LitterEntryType.FullChange, "Full change done", new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc));
        _litterLogRepository.GetLatestFullChangeAsync(catId).Returns(log);

        // Act
        var result = await _sut.GetLatestFullChangeAsync(catId);

        // Assert
        result.Should().NotBeNull();
        result!.CatId.Should().Be(catId);
        result.EntryType.Should().Be(LitterEntryType.FullChange);
        result.Notes.Should().Be("Full change done");
    }

    // GetLatestFullChangeAsync must return null when no full-change log exists,
    // verifying the null propagation path is handled without throwing.
    [Test]
    public async Task GetLatestFullChangeAsync_NoFullChangeLog_ReturnsNull()
    {
        // Arrange
        var catId = Guid.NewGuid();
        _litterLogRepository.GetLatestFullChangeAsync(catId).Returns((LitterLog?)null);

        // Act
        var result = await _sut.GetLatestFullChangeAsync(catId);

        // Assert
        result.Should().BeNull();
    }

    // DeleteAsync must forward the id to the repository, confirming the service
    // does not silently swallow the call or substitute a different id.
    [Test]
    public async Task DeleteAsync_ValidId_CallsRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        await _sut.DeleteAsync(id);

        // Assert
        await _litterLogRepository.Received(1).DeleteAsync(id);
    }
}
