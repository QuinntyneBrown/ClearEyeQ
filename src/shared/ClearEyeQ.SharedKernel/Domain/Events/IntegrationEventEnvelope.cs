using System.Text.Json;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.SharedKernel.Domain.Events;

/// <summary>
/// Schema-versioned envelope for integration events published to Azure Service Bus.
/// Contains all metadata required for routing, deduplication, and replay.
/// </summary>
public sealed record IntegrationEventEnvelope(
    Guid EventId,
    string EventType,
    int SchemaVersion,
    TenantId TenantId,
    Guid SubjectId,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAtUtc,
    string PayloadJson)
{
    /// <summary>
    /// Creates a new integration event envelope from a typed payload.
    /// </summary>
    /// <typeparam name="T">The type of the event payload.</typeparam>
    /// <param name="payload">The event payload to serialize.</param>
    /// <param name="tenantId">The tenant scope.</param>
    /// <param name="subjectId">The subject entity identifier.</param>
    /// <param name="correlationId">The correlation identifier for distributed tracing.</param>
    /// <param name="causationId">The causation identifier linking to the triggering event or command.</param>
    /// <param name="schemaVersion">The schema version of the event payload.</param>
    public static IntegrationEventEnvelope Create<T>(
        T payload,
        TenantId tenantId,
        Guid subjectId,
        Guid correlationId,
        Guid causationId,
        int schemaVersion = 1)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return new IntegrationEventEnvelope(
            EventId: Guid.NewGuid(),
            EventType: typeof(T).Name,
            SchemaVersion: schemaVersion,
            TenantId: tenantId,
            SubjectId: subjectId,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            PayloadJson: payloadJson);
    }
}
