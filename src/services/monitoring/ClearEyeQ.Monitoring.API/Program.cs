using ClearEyeQ.Monitoring.Application.Commands.IngestWearableData;
using ClearEyeQ.Monitoring.Infrastructure;
using ClearEyeQ.SharedKernel.Application.Behaviors;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// MediatR + pipeline behaviors
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IngestWearableDataCommand).Assembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(IngestWearableDataCommand).Assembly);

// Infrastructure (Cosmos, wearable adapters, sleep scorer)
builder.Services.AddMonitoringInfrastructure(builder.Configuration);

// Auth
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
