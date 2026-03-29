namespace ClearEyeQ.Identity.API.Middleware;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenantId");
            if (tenantClaim is not null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            {
                context.Items["TenantId"] = tenantId;
            }
        }

        await _next(context);
    }
}

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
