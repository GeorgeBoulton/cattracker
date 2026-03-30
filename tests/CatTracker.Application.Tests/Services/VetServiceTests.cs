using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Application.Services;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Tests.Services;

[TestFixture]
public class VetServiceTests
{
    private IVetRecordRepository _vetRecordRepository = null!;
    private IVetService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _vetRecordRepository = Substitute.For<IVetRecordRepository>();
        _sut = new VetService(_vetRecordRepository);
    }

    // CreateAsync must return a VetRecordResponse with all fields mapped from the request,
    // confirming the entity is created via the factory and the mapping covers every property.
    [Test]
    public async Task CreateAsync_ValidRequest_ReturnsResponse()
    {
        var catId = Guid.NewGuid();
        var date = new DateOnly(2025, 6, 1);
        var nextDue = new DateOnly(2026, 6, 1);
        var request = new CreateVetRecordRequest(
            catId,
            VetRecordType.Vaccination,
            date,
            "Annual booster",
            "Happy Paws Clinic",
            "Dr. Smith",
            "No adverse reactions",
            85.50m,
            nextDue);

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.CatId.Should().Be(catId);
        result.RecordType.Should().Be(VetRecordType.Vaccination);
        result.Date.Should().Be(date);
        result.Description.Should().Be("Annual booster");
        result.ClinicName.Should().Be("Happy Paws Clinic");
        result.VetName.Should().Be("Dr. Smith");
        result.Notes.Should().Be("No adverse reactions");
        result.Cost.Should().Be(85.50m);
        result.NextDueDate.Should().Be(nextDue);
        await _vetRecordRepository.Received(1).AddAsync(Arg.Any<VetRecord>());
    }

    // GetByCatAsync must return all records for a cat mapped to responses, confirming
    // list mapping works correctly and preserves the count and field values.
    [Test]
    public async Task GetByCatAsync_CatWithRecords_ReturnsMappedResponses()
    {
        var catId = Guid.NewGuid();
        var records = new List<VetRecord>
        {
            VetRecord.Create(catId, VetRecordType.Visit, new DateOnly(2025, 1, 10), "Annual checkup"),
            VetRecord.Create(catId, VetRecordType.Vaccination, new DateOnly(2025, 3, 15), "Rabies vaccine"),
        };
        _vetRecordRepository.GetByCatAsync(catId).Returns(records);

        var result = await _sut.GetByCatAsync(catId);

        result.Should().HaveCount(2);
        result[0].Description.Should().Be("Annual checkup");
        result[0].RecordType.Should().Be(VetRecordType.Visit);
        result[1].Description.Should().Be("Rabies vaccine");
        result[1].RecordType.Should().Be(VetRecordType.Vaccination);
    }

    // GetByIdAsync must return the mapped response when the repository finds the record,
    // confirming the non-null path through the service maps all fields correctly.
    [Test]
    public async Task GetByIdAsync_ExistingRecord_ReturnsMappedResponse()
    {
        var catId = Guid.NewGuid();
        var record = VetRecord.Create(catId, VetRecordType.Procedure, new DateOnly(2025, 5, 20), "Dental cleaning", "City Vet");
        _vetRecordRepository.GetByIdAsync(record.Id).Returns(record);

        var result = await _sut.GetByIdAsync(record.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(record.Id);
        result.CatId.Should().Be(catId);
        result.Description.Should().Be("Dental cleaning");
        result.ClinicName.Should().Be("City Vet");
        result.RecordType.Should().Be(VetRecordType.Procedure);
    }

    // GetByIdAsync must return null when the repository returns null, verifying
    // that the null propagation path is handled without throwing an exception.
    [Test]
    public async Task GetByIdAsync_NonExistingRecord_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _vetRecordRepository.GetByIdAsync(id).Returns((VetRecord?)null);

        var result = await _sut.GetByIdAsync(id);

        result.Should().BeNull();
    }

    // GetUpcomingAsync must delegate to the repository with the provided date and return
    // the mapped results, confirming the service correctly forwards the upToDate filter.
    [Test]
    public async Task GetUpcomingAsync_WithDate_ReturnsFilteredRecords()
    {
        var catId = Guid.NewGuid();
        var upToDate = new DateOnly(2025, 12, 31);
        var records = new List<VetRecord>
        {
            VetRecord.Create(catId, VetRecordType.Vaccination, new DateOnly(2025, 9, 1), "Flu shot", nextDueDate: new DateOnly(2025, 11, 1)),
        };
        _vetRecordRepository.GetUpcomingAsync(catId, upToDate).Returns(records);

        var result = await _sut.GetUpcomingAsync(catId, upToDate);

        result.Should().HaveCount(1);
        result[0].Description.Should().Be("Flu shot");
        await _vetRecordRepository.Received(1).GetUpcomingAsync(catId, upToDate);
    }

    // UpdateAsync must fetch the record, apply the new values, and call the repository's
    // UpdateAsync, confirming the full update flow is orchestrated correctly by the service.
    [Test]
    public async Task UpdateAsync_ExistingRecord_CallsRepositoryUpdate()
    {
        var catId = Guid.NewGuid();
        var record = VetRecord.Create(catId, VetRecordType.Visit, new DateOnly(2025, 1, 1), "Initial visit");
        _vetRecordRepository.GetByIdAsync(record.Id).Returns(record);

        var updateRequest = new UpdateVetRecordRequest(
            VetRecordType.WeighIn,
            new DateOnly(2025, 7, 1),
            "Weight check",
            "New Clinic",
            "Dr. Jones",
            "Healthy weight",
            20.00m,
            null);

        await _sut.UpdateAsync(record.Id, updateRequest);

        await _vetRecordRepository.Received(1).UpdateAsync(record);
        record.RecordType.Should().Be(VetRecordType.WeighIn);
        record.Description.Should().Be("Weight check");
        record.ClinicName.Should().Be("New Clinic");
    }

    // UpdateAsync must throw KeyNotFoundException when no record exists with the given id,
    // ensuring callers can distinguish a missing record from other failures.
    [Test]
    public async Task UpdateAsync_NonExistingRecord_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _vetRecordRepository.GetByIdAsync(id).Returns((VetRecord?)null);

        var updateRequest = new UpdateVetRecordRequest(
            VetRecordType.Visit,
            new DateOnly(2025, 1, 1),
            "Some description",
            null, null, null, null, null);

        var act = async () => await _sut.UpdateAsync(id, updateRequest);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _vetRecordRepository.DidNotReceive().UpdateAsync(Arg.Any<VetRecord>());
    }

    // DeleteAsync must forward the exact id to the repository, confirming the service
    // does not silently swallow the call or substitute a different id.
    [Test]
    public async Task DeleteAsync_ValidId_CallsRepository()
    {
        var id = Guid.NewGuid();

        await _sut.DeleteAsync(id);

        await _vetRecordRepository.Received(1).DeleteAsync(id);
    }
}
