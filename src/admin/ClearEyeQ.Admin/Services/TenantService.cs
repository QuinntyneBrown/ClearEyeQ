using System.Net.Http.Json;

namespace ClearEyeQ.Admin.Services;

public sealed class TenantService
{
    private readonly HttpClient _http;

    public TenantService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("GatewayApi");
    }

    public async Task<List<TenantDto>> GetTenantsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<TenantDto>>("/api/identity/tenants");
            return result ?? new List<TenantDto>();
        }
        catch (HttpRequestException)
        {
            return new List<TenantDto>();
        }
    }

    public async Task<TenantDto?> GetTenantAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<TenantDto>($"/api/identity/tenants/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> CreateTenantAsync(string name)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/identity/tenants", new { Name = name });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<bool> UpdateTenantAsync(Guid id, string name, string status)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/identity/tenants/{id}", new { Name = name, Status = status });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<bool> DeactivateTenantAsync(Guid id)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"/api/identity/tenants/{id}/deactivate", new { });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}

public sealed class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public int UsersCount { get; set; }
    public string? SubscriptionTier { get; set; }
}
