using System.Text;
using ClearEyeQ.Identity.API.Middleware;
using ClearEyeQ.Identity.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<ClearEyeQ.Identity.Application.Commands.RegisterUser.RegisterUserCommand>();
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ClearEyeQ.Identity.Application.Commands.RegisterUser.RegisterUserValidator>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "default-development-key-that-is-at-least-32-bytes-long!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ClearEyeQ";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ClearEyeQ";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Identity Infrastructure (Cosmos, Redis, Auth providers)
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ClearEyeQ Identity & Access API",
        Version = "v1",
        Description = "Authentication, authorization, and consent management for ClearEyeQ."
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantResolution();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
