using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------
builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);

// ---------------------------------------------------------------------------
// Authentication — JWT Bearer
// ---------------------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["SigningKey"] ?? throw new InvalidOperationException("JWT signing key is not configured.")))
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// Rate Limiting — Fixed Window
// ---------------------------------------------------------------------------
var rateLimitSection = builder.Configuration.GetSection("RateLimiting");

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = rateLimitSection.GetValue("PermitLimit", 100);
        limiterOptions.Window = TimeSpan.FromSeconds(rateLimitSection.GetValue("WindowSeconds", 60));
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = rateLimitSection.GetValue("QueueLimit", 10);
    });
});

// ---------------------------------------------------------------------------
// CORS
// ---------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ---------------------------------------------------------------------------
// Health Checks
// ---------------------------------------------------------------------------
builder.Services.AddHealthChecks();

// ---------------------------------------------------------------------------
// Swagger / OpenAPI
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ClearEyeQ API Gateway",
        Version = "v1",
        Description = "Unified API gateway for the ClearEyeQ ocular health platform."
    });
});

// ---------------------------------------------------------------------------
// YARP Reverse Proxy
// ---------------------------------------------------------------------------
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ---------------------------------------------------------------------------
// Build & Configure Pipeline
// ---------------------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ClearEyeQ API Gateway v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();

// Make the implicit Program class accessible for integration tests
namespace ClearEyeQ.Gateway
{
    public partial class Program;
}
