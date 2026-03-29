using System.Net.Http.Json;

namespace ClearEyeQ.Admin.Services;

public sealed class UserService
{
    private readonly HttpClient _http;

    public UserService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("GatewayApi");
    }

    public async Task<List<UserDto>> GetUsersAsync(string? role = null, string? status = null)
    {
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrEmpty(role)) query.Add($"role={Uri.EscapeDataString(role)}");
            if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");

            var url = "/api/identity/users";
            if (query.Count > 0) url += "?" + string.Join("&", query);

            var result = await _http.GetFromJsonAsync<List<UserDto>>(url);
            return result ?? new List<UserDto>();
        }
        catch (HttpRequestException)
        {
            return new List<UserDto>();
        }
    }

    public async Task<UserDto?> GetUserAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<UserDto>($"/api/identity/users/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> UpdateUserRoleAsync(Guid id, string role)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/identity/users/{id}/role", new { Role = role });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<bool> UpdateUserStatusAsync(Guid id, string status)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/identity/users/{id}/status", new { Status = status });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Patient";
    public string Status { get; set; } = "Active";
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
