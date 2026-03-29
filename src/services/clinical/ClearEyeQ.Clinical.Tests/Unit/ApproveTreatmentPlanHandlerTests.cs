using ClearEyeQ.Clinical.Application.Commands.ApproveTreatmentPlan;
using ClearEyeQ.Clinical.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.Events;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ClearEyeQ.Clinical.Tests.Unit;

public sealed class ApproveTreatmentPlanHandlerTests
{
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ApproveTreatmentPlanHandler _handler;

    public ApproveTreatmentPlanHandlerTests()
    {
        _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _handler = new ApproveTreatmentPlanHandler(_eventPublisher);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesTreatmentPlanApprovedEvent()
    {
        // Arrange
        var command = new ApproveTreatmentPlanCommand(
            TenantId: Guid.NewGuid(),
            TreatmentReviewId: Guid.NewGuid(),
            ClinicianId: "clinician-001",
            Rationale: "Treatment plan is appropriate for the patient's condition.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<IntegrationEventEnvelope>(e =>
                e.EventType == "TreatmentPlanApprovedPayload" &&
                e.SubjectId == command.TreatmentReviewId &&
                e.TenantId.Value == command.TenantId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUnit()
    {
        // Arrange
        var command = new ApproveTreatmentPlanCommand(
            TenantId: Guid.NewGuid(),
            TreatmentReviewId: Guid.NewGuid(),
            ClinicianId: "clinician-002",
            Rationale: "Approved after review.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
    }

    [Fact]
    public async Task Handle_PublishFails_ThrowsException()
    {
        // Arrange
        var command = new ApproveTreatmentPlanCommand(
            TenantId: Guid.NewGuid(),
            TreatmentReviewId: Guid.NewGuid(),
            ClinicianId: "clinician-003",
            Rationale: "Approved.");

        _eventPublisher
            .PublishAsync(Arg.Any<IntegrationEventEnvelope>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Service Bus unavailable")));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Service Bus unavailable");
    }
}
