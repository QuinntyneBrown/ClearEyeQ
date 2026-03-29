using ClearEyeQ.Treatment.Application.Commands.EvaluateEscalation;
using ClearEyeQ.Treatment.Application.Interfaces;
using ClearEyeQ.Treatment.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Treatment.Worker.Workers;

public sealed class ClosedLoopMonitorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ClosedLoopMonitorWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public ClosedLoopMonitorWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ClosedLoopMonitorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Closed-loop monitor worker starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluateActivePlansAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error during closed-loop monitoring cycle");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Closed-loop monitor worker stopping");
    }

    private async Task EvaluateActivePlansAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITreatmentPlanRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var calculator = scope.ServiceProvider.GetRequiredService<IEfficacyCalculator>();

        // Evaluate all active plans across tenants
        // In production, this would be scoped per tenant with proper tenant iteration
        var activePlans = await repository.GetActivePlansAsync(
            default, // Would iterate over tenants in production
            ct);

        _logger.LogInformation("Evaluating {Count} active treatment plans", activePlans.Count);

        foreach (var plan in activePlans)
        {
            try
            {
                if (plan.Status != TreatmentStatus.Active)
                    continue;

                if (calculator.ShouldEscalate(plan))
                {
                    _logger.LogInformation(
                        "Escalation triggered for plan {PlanId} in tenant {TenantId}",
                        plan.PlanId, plan.TenantId);

                    await mediator.Send(new EvaluateEscalationCommand(
                        plan.PlanId,
                        plan.TenantId.Value), ct);
                }
                else if (calculator.IsResolved(plan))
                {
                    _logger.LogInformation(
                        "Resolution detected for plan {PlanId} in tenant {TenantId}",
                        plan.PlanId, plan.TenantId);

                    plan.VerifyResolution();
                    if (plan.Status == TreatmentStatus.Resolved)
                    {
                        plan.TransitionToMaintenance();
                        await repository.UpdateAsync(plan, ct);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error evaluating plan {PlanId}", plan.PlanId);
            }
        }
    }
}
