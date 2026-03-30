using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Application.Services;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Tests.Services;

[TestFixture]
public class WaterServiceTests
{
    private IWaterLogRepository _waterLogRepository = null!;
    private IWaterService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _waterLogRepository = Substitute.For<IWaterLogRepository>();
        _sut = new WaterService(_waterLogRepository);
    }

    // LogWaterAsync must return a WaterLogResponse with fields mapped from the request,
    // confirming the entity is created and the mapping is correct.
    [Test]
    public async Task LogWaterAsync_ValidRequest_ReturnsResponse()
    {
        var catId = Guid.NewGuid();
        var cleanedAt = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var request = new LogWaterRequest(catId, "Fresh bowl", cleanedAt);

        var result = await _sut.LogWaterAsync(request);

        result.Should().NotBeNull();
        result.CatId.Should().Be(catId);
        result.Notes.Should().Be("Fresh bowl");
        result.CleanedAt.Should().Be(cleanedAt);
        await _waterLogRepository.Received(1).AddAsync(Arg.Any<WaterLog>());
    }

    // LogWaterAsync with a null CleanedAt must still produce a response with a CleanedAt
    // value set, confirming the entity factory defaults to UtcNow rather than leaving it unset.
    [Test]
    public async Task LogWaterAsync_WithNullCleanedAt_UsesCurrentTime()
    {
        var catId = Guid.NewGuid();
        var before = DateTime.UtcNow;
        var request = new LogWaterRequest(catId, null, null);

        var result = await _sut.LogWaterAsync(request);

        var after = DateTime.UtcNow;
        result.CleanedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        await _waterLogRepository.Received(1).AddAsync(Arg.Any<WaterLog>());
    }

    // GetRecentAsync must return all water logs mapped to responses, confirming
    // list mapping works correctly and preserves all entries.
    [Test]
    public async Task GetRecentAsync_CatWithLogs_ReturnsMappedResponses()
    {
        var catId = Guid.NewGuid();
        var logs = new List<WaterLog>
        {
            WaterLog.Create(catId, "First bowl", new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc)),
            WaterLog.Create(catId, "Second bowl", new DateTime(2025, 6, 1, 18, 0, 0, DateTimeKind.Utc)),
        };
        _waterLogRepository.GetRecentAsync(catId, 20).Returns(logs);

        var result = await _sut.GetRecentAsync(catId);

        result.Should().HaveCount(2);
        result[0].Notes.Should().Be("First bowl");
        result[1].Notes.Should().Be("Second bowl");
    }

    // GetLatestAsync must return the most recent water log mapped to a response,
    // confirming the mapping path for a non-null repository result works correctly.
    [Test]
    public async Task GetLatestAsync_CatWithLogs_ReturnsMostRecent()
    {
        var catId = Guid.NewGuid();
        var log = WaterLog.Create(catId, "Latest bowl", new DateTime(2025, 6, 1, 18, 0, 0, DateTimeKind.Utc));
        _waterLogRepository.GetLatestAsync(catId).Returns(log);

        var result = await _sut.GetLatestAsync(catId);

        result.Should().NotBeNull();
        result!.CatId.Should().Be(catId);
        result.Notes.Should().Be("Latest bowl");
    }

    // GetLatestAsync must return null when the repository returns null, verifying
    // that the null propagation path is handled without throwing.
    [Test]
    public async Task GetLatestAsync_CatWithNoLogs_ReturnsNull()
    {
        var catId = Guid.NewGuid();
        _waterLogRepository.GetLatestAsync(catId).Returns((WaterLog?)null);

        var result = await _sut.GetLatestAsync(catId);

        result.Should().BeNull();
    }

    // DeleteAsync must forward the id to the repository, confirming the service
    // does not silently swallow the call or substitute a different id.
    [Test]
    public async Task DeleteAsync_ValidId_CallsRepository()
    {
        var id = Guid.NewGuid();

        await _sut.DeleteAsync(id);

        await _waterLogRepository.Received(1).DeleteAsync(id);
    }
}
