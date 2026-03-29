using ClearEyeQ.Treatment.Infrastructure;
using ClearEyeQ.Treatment.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ClearEyeQ.Treatment.Application.Commands.CreateTreatmentPlan.CreateTreatmentPlanCommand).Assembly));

builder.Services.AddTreatmentInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ClosedLoopMonitorWorker>();

var host = builder.Build();
host.Run();
