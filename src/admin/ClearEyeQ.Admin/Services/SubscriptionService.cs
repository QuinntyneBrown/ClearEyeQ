using System.Net.Http.Json;

namespace ClearEyeQ.Admin.Services;

public sealed class SubscriptionService
{
    private readonly HttpClient _http;

    public SubscriptionService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("GatewayApi");
    }

    public async Task<List<SubscriptionDto>> GetSubscriptionsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<SubscriptionDto>>("/api/billing/subscriptions");
            return result ?? new List<SubscriptionDto>();
        }
        catch (HttpRequestException)
        {
            return new List<SubscriptionDto>();
        }
    }

    public async Task<PlanDistributionDto> GetPlanDistributionAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<PlanDistributionDto>("/api/billing/subscriptions/distribution");
            return result ?? new PlanDistributionDto();
        }
        catch (HttpRequestException)
        {
            return new PlanDistributionDto();
        }
    }

    public async Task<RevenueMetricsDto> GetRevenueMetricsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<RevenueMetricsDto>("/api/billing/revenue/metrics");
            return result ?? new RevenueMetricsDto();
        }
        catch (HttpRequestException)
        {
            return new RevenueMetricsDto();
        }
    }
}

public sealed class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Tier { get; set; } = "Free";
    public string Status { get; set; } = "Active";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MonthlyAmount { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? PreviousTier { get; set; }
}

public sealed class PlanDistributionDto
{
    public int FreeCount { get; set; }
    public int ProCount { get; set; }
    public int PremiumCount { get; set; }
    public int AutonomousCount { get; set; }
    public int TotalCount => FreeCount + ProCount + PremiumCount + AutonomousCount;
}

public sealed class RevenueMetricsDto
{
    public decimal MonthlyRecurring { get; set; }
    public decimal AnnualRecurring { get; set; }
    public decimal AverageRevenuePerUser { get; set; }
    public decimal ChurnRate { get; set; }
}
