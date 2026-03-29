using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.Clinical.Application.Queries.GetPatientList;
using ClearEyeQ.Clinical.Application.ReadModels;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ClearEyeQ.Clinical.Tests.Unit;

public sealed class GetPatientListHandlerTests
{
    private readonly IPatientReadModelStore _store;
    private readonly GetPatientListHandler _handler;

    public GetPatientListHandlerTests()
    {
        _store = Substitute.For<IPatientReadModelStore>();
        _handler = new GetPatientListHandler(_store);
    }

    [Fact]
    public async Task Handle_ReturnsPatientSummaryDtos()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var patients = new List<PatientSummaryReadModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PatientId = Guid.NewGuid(),
                Name = "Jane Doe",
                LastScanDate = DateTimeOffset.UtcNow.AddDays(-1),
                RednessScore = 3.5,
                Status = "Active",
                UpdatedAtUtc = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PatientId = Guid.NewGuid(),
                Name = "John Smith",
                LastScanDate = DateTimeOffset.UtcNow.AddDays(-3),
                RednessScore = 1.2,
                Status = "Active",
                UpdatedAtUtc = DateTimeOffset.UtcNow
            }
        };

        _store.GetPatientListAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(patients);

        // Act
        var result = await _handler.Handle(new GetPatientListQuery(tenantId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Jane Doe");
        result[0].RednessScore.Should().Be(3.5);
        result[1].Name.Should().Be("John Smith");
        result[1].Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_EmptyStore_ReturnsEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _store.GetPatientListAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(new List<PatientSummaryReadModel>());

        // Act
        var result = await _handler.Handle(new GetPatientListQuery(tenantId), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsAllFields()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var lastScan = DateTimeOffset.UtcNow.AddHours(-6);
        var patients = new List<PatientSummaryReadModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PatientId = patientId,
                Name = "Test Patient",
                LastScanDate = lastScan,
                RednessScore = 7.8,
                Status = "NeedsReview",
                UpdatedAtUtc = DateTimeOffset.UtcNow
            }
        };

        _store.GetPatientListAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(patients);

        // Act
        var result = await _handler.Handle(new GetPatientListQuery(tenantId), CancellationToken.None);

        // Assert
        var dto = result.Single();
        dto.PatientId.Should().Be(patientId);
        dto.Name.Should().Be("Test Patient");
        dto.LastScanDate.Should().Be(lastScan);
        dto.RednessScore.Should().Be(7.8);
        dto.Status.Should().Be("NeedsReview");
    }
}
