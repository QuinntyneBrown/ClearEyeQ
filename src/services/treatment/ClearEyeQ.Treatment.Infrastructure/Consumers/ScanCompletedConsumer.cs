using ClearEyeQ.SharedKernel.Infrastructure.Messaging;
using ClearEyeQ.Treatment.Application.Commands.RecordEfficacy;
using ClearEyeQ.Treatment.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearEyeQ.Treatment.Infrastructure.Consumers;

public sealed record ScanCompletedMessage(
    Guid ScanId,
    Guid UserId,
    Guid TenantId,
    double RednessScore,
    double BaselineScore);

public sealed class ScanCompletedConsumer : InboxConsumer<ScanCompletedMessage>
{
    private readonly IMediator _mediator;
    private readonly ITreatmentPlanRepository _repository;

    public ScanCompletedConsumer(
        IConnectionMultiplexer redis,
        IMediator mediator,
        ITreatmentPlanRepository repository,
        ILogger<ScanCompletedConsumer> logger)
        : base(redis, logger)
    {
        _mediator = mediator;
        _repository = repository;
    }

    protected override async Task HandleAsync(ScanCompletedMessage message, CancellationToken ct)
    {
        var userId = new UserId(message.UserId);
        var tenantId = new TenantId(message.TenantId);

        var activePlan = await _repository.GetActivePlanAsync(userId, tenantId, ct);
        if (activePlan is null)
            return;

        var command = new RecordEfficacyCommand(
            PlanId: activePlan.PlanId,
            TenantId: message.TenantId,
            RednessScore: message.RednessScore,
            BaselineScore: message.BaselineScore);

        await _mediator.Send(command, ct);
    }
}
