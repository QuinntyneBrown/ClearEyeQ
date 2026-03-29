using System.Net.Http.Json;

namespace ClearEyeQ.Admin.Services;

public sealed class FeatureFlagService
{
    private readonly HttpClient _http;

    public FeatureFlagService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("GatewayApi");
    }

    public async Task<List<FeatureFlagDto>> GetFlagsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<FeatureFlagDto>>("/api/identity/features");
            return result ?? new List<FeatureFlagDto>();
        }
        catch (HttpRequestException)
        {
            return new List<FeatureFlagDto>();
        }
    }

    public async Task<bool> ToggleFlagAsync(string flagName, bool enabled)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/identity/features/{Uri.EscapeDataString(flagName)}", new { Enabled = enabled });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<bool> SetTenantOverrideAsync(string flagName, Guid tenantId, bool enabled)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                $"/api/identity/features/{Uri.EscapeDataString(flagName)}/tenants/{tenantId}",
                new { Enabled = enabled });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}

public sealed class FeatureFlagDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool GlobalDefault { get; set; }
    public List<TenantOverrideDto> TenantOverrides { get; set; } = new();
}

public sealed class TenantOverrideDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}
