using System.Net;
using System.Text.Json;
using ClearEyeQ.Identity.Application.Interfaces;
using ClearEyeQ.Identity.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Azure.Cosmos;

namespace ClearEyeQ.Identity.Infrastructure.Persistence;

public sealed class CosmosUserRepository : IUserRepository
{
    private readonly Container _container;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CosmosUserRepository(CosmosClient cosmosClient, string databaseName)
    {
        _container = cosmosClient.GetContainer(databaseName, "users");
    }

    public async Task<User?> GetByIdAsync(UserId userId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(tenantId.Value.ToString());
            var response = await _container.ReadItemAsync<JsonElement>(
                userId.Value.ToString(),
                partitionKey,
                cancellationToken: cancellationToken);

            return DeserializeUser(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<User?> GetByEmailAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.email = @email")
            .WithParameter("@email", email.ToLowerInvariant());

        var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(tenantId.Value.ToString());
        using var iterator = _container.GetItemQueryIterator<JsonElement>(query, requestOptions: new QueryRequestOptions
        {
            PartitionKey = partitionKey,
            MaxItemCount = 1
        });

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            var item = response.FirstOrDefault();
            if (item.ValueKind != JsonValueKind.Undefined)
            {
                return DeserializeUser(item);
            }
        }

        return null;
    }

    public async Task SaveAsync(User user, CancellationToken cancellationToken = default)
    {
        var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(user.TenantId.Value.ToString());
        var document = SerializeUser(user);

        await _container.UpsertItemAsync(
            document,
            partitionKey,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.email = @email")
            .WithParameter("@email", email.ToLowerInvariant());

        var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(tenantId.Value.ToString());
        using var iterator = _container.GetItemQueryIterator<int>(query, requestOptions: new QueryRequestOptions
        {
            PartitionKey = partitionKey
        });

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            return response.FirstOrDefault() > 0;
        }

        return false;
    }

    private static JsonElement SerializeUser(User user)
    {
        var document = new Dictionary<string, object?>
        {
            ["id"] = user.Id.ToString(),
            ["tenantId"] = user.TenantId.Value.ToString(),
            ["email"] = user.Email,
            ["passwordHash"] = user.PasswordHash,
            ["salt"] = user.Salt,
            ["displayName"] = user.DisplayName,
            ["role"] = user.Role.ToString(),
            ["accountStatus"] = user.AccountStatus.ToString(),
            ["mfaEnabled"] = user.MfaEnabled,
            ["failedLoginAttempts"] = user.FailedLoginAttempts,
            ["lockedUntil"] = user.LockedUntil,
            ["consents"] = user.Consents.Select(c => new Dictionary<string, object?>
            {
                ["consentId"] = c.ConsentId.ToString(),
                ["consentType"] = c.ConsentType.ToString(),
                ["grantedAt"] = c.GrantedAt,
                ["revokedAt"] = c.RevokedAt,
                ["isActive"] = c.IsActive
            }).ToList(),
            ["refreshTokens"] = user.RefreshTokens.Select(t => new Dictionary<string, object?>
            {
                ["tokenHash"] = t.TokenHash,
                ["deviceFingerprint"] = t.DeviceFingerprint,
                ["issuedAt"] = t.IssuedAt,
                ["expiresAt"] = t.ExpiresAt,
                ["isRevoked"] = t.IsRevoked
            }).ToList(),
            ["audit"] = new Dictionary<string, object?>
            {
                ["createdAt"] = user.Audit?.CreatedAt,
                ["createdBy"] = user.Audit?.CreatedBy,
                ["modifiedAt"] = user.Audit?.ModifiedAt,
                ["modifiedBy"] = user.Audit?.ModifiedBy
            }
        };

        var json = JsonSerializer.Serialize(document, SerializerOptions);
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private static User? DeserializeUser(JsonElement element)
    {
        var json = element.GetRawText();
        // Use reflection-based deserialization with a factory approach.
        // In production, a dedicated Cosmos DB serializer or source generator would be used.
        return JsonSerializer.Deserialize<User>(json, SerializerOptions);
    }
}
