using ClearEyeQ.Environmental.Application.Commands.CaptureSnapshot;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Environmental.Infrastructure.Workers;

/// <summary>
/// Background service that periodically collects environmental data for active users.
/// Runs on a configurable timer interval (default: 30 minutes).
/// </summary>
public sealed class EnvironmentalCollectionWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<EnvironmentalCollectionWorker> logger) : BackgroundService
{
    private TimeSpan CollectionInterval =>
        TimeSpan.FromMinutes(configuration.GetValue("Environmental:CollectionIntervalMinutes", 30));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Environmental collection worker started with interval: {Interval}.", CollectionInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectForActiveUsersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during environmental data collection cycle.");
            }

            await Task.Delay(CollectionInterval, stoppingToken);
        }

        logger.LogInformation("Environmental collection worker stopped.");
    }

    private async Task CollectForActiveUsersAsync(CancellationToken ct)
    {
        // In production, this would query a user service for active users and their locations.
        // For now, we rely on the snapshot capture being triggered per-user via the API
        // or by an external scheduler that calls the API for each active user.
        var activeUsers = await GetActiveUsersAsync(ct);

        logger.LogInformation("Starting environmental collection for {Count} active users.", activeUsers.Count);

        foreach (var user in activeUsers)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = new CaptureSnapshotCommand(
                    UserId: user.UserId,
                    TenantId: user.TenantId,
                    Latitude: user.Latitude,
                    Longitude: user.Longitude);

                await mediator.Send(command, ct);

                logger.LogDebug("Environmental snapshot captured for user {UserId}.", user.UserId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Failed to capture environmental snapshot for user {UserId}.", user.UserId);
            }
        }
    }

    private Task<IReadOnlyList<ActiveUserLocation>> GetActiveUsersAsync(CancellationToken ct)
    {
        // This is a placeholder that would integrate with the Identity & Access service
        // to retrieve active users who have opted in to environmental monitoring.
        // The actual implementation would query a user preferences table or cache.
        IReadOnlyList<ActiveUserLocation> users = [];
        return Task.FromResult(users);
    }

    private sealed record ActiveUserLocation(Guid UserId, Guid TenantId, double Latitude, double Longitude);
}
