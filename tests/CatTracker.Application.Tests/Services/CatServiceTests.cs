using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Application.Services;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Tests.Services;

[TestFixture]
public class CatServiceTests
{
    private ICatRepository _catRepository = null!;
    private ICatService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _catRepository = Substitute.For<ICatRepository>();
        _sut = new CatService(_catRepository);
    }

    // GetCatAsync must return a populated CatResponse when the repository holds a cat
    // with the requested id — confirms the happy-path mapping is correct.
    [Test]
    public async Task GetCatAsync_CatExists_ReturnsMappedCatResponse()
    {
        var cat = Cat.Create("Whiskers", "Persian", new DateOnly(2020, 3, 15), "https://example.com/photo.jpg");
        _catRepository.GetByIdAsync(cat.Id).Returns(cat);

        var result = await _sut.GetCatAsync(cat.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(cat.Id);
        result.Name.Should().Be("Whiskers");
        result.Breed.Should().Be("Persian");
        result.DateOfBirth.Should().Be(new DateOnly(2020, 3, 15));
        result.PhotoUrl.Should().Be("https://example.com/photo.jpg");
        result.IsActive.Should().BeTrue();
    }

    // GetCatAsync must return null when no cat exists for the given id so that callers
    // (e.g. controllers) can respond with 404 rather than throwing.
    [Test]
    public async Task GetCatAsync_CatNotFound_ReturnsNull()
    {
        var missingId = Guid.NewGuid();
        _catRepository.GetByIdAsync(missingId).Returns((Cat?)null);

        var result = await _sut.GetCatAsync(missingId);

        result.Should().BeNull();
    }

    // GetAllActiveCatsAsync must map every active cat returned by the repository to a
    // CatResponse, preserving count and field values — confirms bulk mapping works.
    [Test]
    public async Task GetAllActiveCatsAsync_CatsExist_ReturnsMappedList()
    {
        var cats = new List<Cat>
        {
            Cat.Create("Luna", "Siamese", null, null),
            Cat.Create("Mochi", "Maine Coon", new DateOnly(2021, 6, 1), null),
        };
        _catRepository.GetAllActiveAsync().Returns(cats);

        var result = await _sut.GetAllActiveCatsAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Luna");
        result[1].Name.Should().Be("Mochi");
    }

    // UpdateCatAsync must throw KeyNotFoundException when the cat is absent so that
    // upstream code can distinguish "not found" from other failure modes.
    [Test]
    public async Task UpdateCatAsync_CatNotFound_ThrowsKeyNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _catRepository.GetByIdAsync(missingId).Returns((Cat?)null);

        var request = new UpdateCatRequest("NewName", null, null, null);

        Func<Task> act = () => _sut.UpdateCatAsync(missingId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // UpdateCatAsync must mutate the cat via Update() and persist it via
    // repository.UpdateAsync() — verifies the full orchestration path for a valid update.
    [Test]
    public async Task UpdateCatAsync_CatExists_CallsUpdateAndPersists()
    {
        var cat = Cat.Create("OldName", "Tabby", new DateOnly(2019, 1, 1), null);
        _catRepository.GetByIdAsync(cat.Id).Returns(cat);

        var request = new UpdateCatRequest("NewName", "Ragdoll", new DateOnly(2019, 5, 10), "https://example.com/new.jpg");

        await _sut.UpdateCatAsync(cat.Id, request);

        // Verify the domain object was mutated with the requested values
        cat.Name.Should().Be("NewName");
        cat.Breed.Should().Be("Ragdoll");
        cat.DateOfBirth.Should().Be(new DateOnly(2019, 5, 10));
        cat.PhotoUrl.Should().Be("https://example.com/new.jpg");

        // Verify the repository was called to persist the change
        await _catRepository.Received(1).UpdateAsync(cat);
    }
}
