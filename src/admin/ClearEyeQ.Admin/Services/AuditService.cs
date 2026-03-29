using System.Net.Http.Json;

namespace ClearEyeQ.Admin.Services;

public sealed class AuditService
{
    private readonly HttpClient _http;

    public AuditService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("GatewayApi");
    }

    public async Task<AuditLogPageDto> SearchAuditLogsAsync(
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? action = null,
        Guid? userId = null,
        Guid? tenantId = null,
        int page = 1,
        int pageSize = 25)
    {
        try
        {
            var query = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (dateFrom.HasValue) query.Add($"dateFrom={dateFrom.Value:O}");
            if (dateTo.HasValue) query.Add($"dateTo={dateTo.Value:O}");
            if (!string.IsNullOrEmpty(action)) query.Add($"action={Uri.EscapeDataString(action)}");
            if (userId.HasValue) query.Add($"userId={userId.Value}");
            if (tenantId.HasValue) query.Add($"tenantId={tenantId.Value}");

            var url = "/api/identity/audit?" + string.Join("&", query);
            var result = await _http.GetFromJsonAsync<AuditLogPageDto>(url);
            return result ?? new AuditLogPageDto();
        }
        catch (HttpRequestException)
        {
            return new AuditLogPageDto();
        }
    }
}

public sealed class AuditLogPageDto
{
    public List<AuditLogDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public sealed class AuditLogDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string? TenantName { get; set; }
    public Guid? TenantId { get; set; }
    public string? Detail { get; set; }
}
