using System.Diagnostics;

namespace ClearEyeQ.Admin.Services;

public sealed class SystemHealthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    private static readonly string[] ServiceNames = new[]
    {
        "Identity", "Scan", "Monitoring", "Environmental",
        "Diagnostic", "Predictive", "Treatment", "Clinical",
        "Notifications", "Billing", "Fhir", "Gateway"
    };

    public SystemHealthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<List<ServiceHealthDto>> GetAllServiceHealthAsync()
    {
        var tasks = ServiceNames.Select(CheckServiceHealthAsync);
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    private async Task<ServiceHealthDto> CheckServiceHealthAsync(string serviceName)
    {
        var baseUrl = _configuration[$"ServiceEndpoints:{serviceName}"];
        if (string.IsNullOrEmpty(baseUrl))
        {
            return new ServiceHealthDto
            {
                Name = serviceName,
                Status = "Unhealthy",
                ResponseTimeMs = -1,
                CheckedAt = DateTime.UtcNow,
                ErrorMessage = "Endpoint not configured"
            };
        }

        var client = _httpClientFactory.CreateClient("HealthCheck");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await client.GetAsync($"{baseUrl}/health");
            stopwatch.Stop();

            return new ServiceHealthDto
            {
                Name = serviceName,
                Status = response.IsSuccessStatusCode ? "Healthy" : "Degraded",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            return new ServiceHealthDto
            {
                Name = serviceName,
                Status = "Unhealthy",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                CheckedAt = DateTime.UtcNow,
                ErrorMessage = "Request timed out"
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return new ServiceHealthDto
            {
                Name = serviceName,
                Status = "Unhealthy",
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                CheckedAt = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }
}

public sealed class ServiceHealthDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Unhealthy";
    public long ResponseTimeMs { get; set; }
    public DateTime CheckedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
