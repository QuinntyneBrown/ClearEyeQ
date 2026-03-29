using ClearEyeQ.Fhir.Application.Interfaces;
using ClearEyeQ.Fhir.Application.Mappers;
using FluentAssertions;
using Hl7.Fhir.Model;
using Xunit;

namespace ClearEyeQ.Fhir.Tests.Unit;

public sealed class ScanToObservationMapperTests
{
    [Fact]
    public void Map_ValidScanData_ReturnsObservationWithCorrectId()
    {
        // Arrange
        var scanId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var scan = new ScanData(scanId, "Left", 3.5, 0.8, 0.95, DateTimeOffset.UtcNow);

        // Act
        var observation = ScanToObservationMapper.Map(scan, patientId);

        // Assert
        observation.Id.Should().Be(scanId.ToString());
        observation.Status.Should().Be(ObservationStatus.Final);
    }

    [Fact]
    public void Map_ValidScanData_SetsSubjectReference()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var scan = new ScanData(Guid.NewGuid(), "Right", 2.0, 0.7, 0.90, DateTimeOffset.UtcNow);

        // Act
        var observation = ScanToObservationMapper.Map(scan, patientId);

        // Assert
        observation.Subject.Reference.Should().Be($"Patient/{patientId}");
    }

    [Fact]
    public void Map_ValidScanData_IncludesAllComponents()
    {
        // Arrange
        var scan = new ScanData(Guid.NewGuid(), "Left", 4.2, 0.6, 0.88, DateTimeOffset.UtcNow);

        // Act
        var observation = ScanToObservationMapper.Map(scan, Guid.NewGuid());

        // Assert
        observation.Component.Should().HaveCount(3);
        observation.Component[0].Code.Coding[0].Code.Should().Be("redness-score");
        observation.Component[1].Code.Coding[0].Code.Should().Be("tear-film-stability");
        observation.Component[2].Code.Coding[0].Code.Should().Be("confidence-score");
    }

    [Fact]
    public void Map_LeftEye_SetsSnomedCodeForLeftEye()
    {
        // Arrange
        var scan = new ScanData(Guid.NewGuid(), "Left", 1.0, 0.9, 0.99, DateTimeOffset.UtcNow);

        // Act
        var observation = ScanToObservationMapper.Map(scan, Guid.NewGuid());

        // Assert
        observation.BodySite.Coding[0].Code.Should().Be("8966001");
    }

    [Fact]
    public void Map_RightEye_SetsSnomedCodeForRightEye()
    {
        // Arrange
        var scan = new ScanData(Guid.NewGuid(), "Right", 1.0, 0.9, 0.99, DateTimeOffset.UtcNow);

        // Act
        var observation = ScanToObservationMapper.Map(scan, Guid.NewGuid());

        // Assert
        observation.BodySite.Coding[0].Code.Should().Be("18944008");
    }

    [Fact]
    public void Map_RednessScoreComponent_HasCorrectValue()
    {
        // Arrange
        var scan = new ScanData(Guid.NewGuid(), "Left", 5.5, 0.4, 0.75, DateTimeOffset.UtcNow);

        // Act
        var observation = ScanToObservationMapper.Map(scan, Guid.NewGuid());

        // Assert
        var rednessComponent = observation.Component[0];
        var quantity = rednessComponent.Value as Quantity;
        quantity.Should().NotBeNull();
        quantity!.Value.Should().Be(5.5m);
    }

    [Fact]
    public void Map_NullScanData_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ScanToObservationMapper.Map(null!, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
