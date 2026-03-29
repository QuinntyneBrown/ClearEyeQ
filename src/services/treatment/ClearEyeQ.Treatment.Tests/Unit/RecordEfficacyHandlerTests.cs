using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using ClearEyeQ.Treatment.Application.Commands.RecordEfficacy;
using ClearEyeQ.Treatment.Application.Interfaces;
using ClearEyeQ.Treatment.Domain.Aggregates;
using ClearEyeQ.Treatment.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ClearEyeQ.Treatment.Tests.Unit;

public sealed class RecordEfficacyHandlerTests
{
    private readonly ITreatmentPlanRepository _repository = Substitute.For<ITreatmentPlanRepository>();
    private readonly IEfficacyCalculator _calculator = Substitute.For<IEfficacyCalculator>();
    private readonly RecordEfficacyHandler _handler;

    public RecordEfficacyHandlerTests()
    {
        _handler = new RecordEfficacyHandler(_repository, _calculator);
    }

    [Fact]
    public async Task Handle_ShouldRecordEfficacyMeasurement()
    {
        var tenantId = TenantId.New();
        var userId = UserId.New();
        var plan = TreatmentPlan.Propose(userId, tenantId, Guid.NewGuid());
        plan.Activate();

        _repository.GetByIdAsync(plan.PlanId, tenantId, Arg.Any<CancellationToken>())
            .Returns(plan);
        _calculator.IsResolved(Arg.Any<TreatmentPlan>()).Returns(false);

        var command = new RecordEfficacyCommand(
            PlanId: plan.PlanId,
            TenantId: tenantId.Value,
            RednessScore: 3.0,
            BaselineScore: 8.0);

        await _handler.Handle(command, CancellationToken.None);

        plan.EfficacyMeasurements.Should().HaveCount(1);
        plan.EfficacyMeasurements[0].DeltaPercent.Should().Be(62.5);
        await _repository.Received(1).UpdateAsync(plan, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPlanNotFound_ShouldThrow()
    {
        var tenantId = TenantId.New();
        _repository.GetByIdAsync(Arg.Any<Guid>(), tenantId, Arg.Any<CancellationToken>())
            .Returns((TreatmentPlan?)null);

        var command = new RecordEfficacyCommand(
            PlanId: Guid.NewGuid(),
            TenantId: tenantId.Value,
            RednessScore: 3.0,
            BaselineScore: 8.0);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenResolved_ShouldVerifyResolution()
    {
        var tenantId = TenantId.New();
        var userId = UserId.New();
        var plan = TreatmentPlan.Propose(userId, tenantId, Guid.NewGuid());
        plan.Activate();

        // Record enough measurements for resolution check
        plan.RecordEfficacy(2.0, 10.0);
        plan.RecordEfficacy(1.5, 10.0);

        _repository.GetByIdAsync(plan.PlanId, tenantId, Arg.Any<CancellationToken>())
            .Returns(plan);
        _calculator.IsResolved(Arg.Any<TreatmentPlan>()).Returns(true);

        var command = new RecordEfficacyCommand(
            PlanId: plan.PlanId,
            TenantId: tenantId.Value,
            RednessScore: 1.0,
            BaselineScore: 10.0);

        await _handler.Handle(command, CancellationToken.None);

        // The handler calls VerifyResolution, which checks average delta >= 80%
        // Our measurements: 80%, 85%, 90% -> average 85% >= 80% -> Resolved
        plan.Status.Should().Be(TreatmentStatus.Resolved);
        await _repository.Received(1).UpdateAsync(plan, Arg.Any<CancellationToken>());
    }
}
