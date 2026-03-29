using ClearEyeQ.Diagnostic.Application.Interfaces;
using ClearEyeQ.Diagnostic.Domain.Aggregates;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Diagnostic.Application.Commands.GenerateDiagnosis;

public sealed class GenerateDiagnosisHandler : IRequestHandler<GenerateDiagnosisCommand, Guid>
{
    private readonly IDiagnosticSessionRepository _repository;
    private readonly IDiagnosticMLClient _mlClient;
    private readonly IMedicationRepository _medicationRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateDiagnosisHandler> _logger;

    public GenerateDiagnosisHandler(
        IDiagnosticSessionRepository repository,
        IDiagnosticMLClient mlClient,
        IMedicationRepository medicationRepository,
        IMediator mediator,
        ILogger<GenerateDiagnosisHandler> logger)
    {
        _repository = repository;
        _mlClient = mlClient;
        _medicationRepository = medicationRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Guid> Handle(GenerateDiagnosisCommand request, CancellationToken ct)
    {
        var scanId = new ScanId(request.ScanId);
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var session = DiagnosticSession.Create(scanId, userId, tenantId);
        await _repository.AddAsync(session, ct);

        _logger.LogInformation(
            "Diagnostic session {SessionId} created for scan {ScanId}, tenant {TenantId}",
            session.SessionId, scanId, tenantId);

        try
        {
            var medications = await _medicationRepository.GetActiveMedicationsAsync(userId, tenantId, ct);

            var environmentalFactors = new List<EnvironmentalFactorInput>();
            var monitoringMetrics = new List<MonitoringMetricInput>();

            var diagnoses = await _mlClient.ClassifyConditionsAsync(
                scanId,
                rednessScore: 0.0,
                zoneScores: new Dictionary<string, double>(),
                tearFilmBreakUpTime: 0.0,
                environmentalFactors,
                monitoringMetrics,
                ct);

            foreach (var diagnosis in diagnoses)
            {
                session.AddDiagnosis(diagnosis);
            }

            var causalGraph = await _mlClient.InferCausalGraphAsync(
                scanId,
                diagnoses,
                environmentalFactors,
                monitoringMetrics,
                medications,
                ct);

            session.SetCausalGraph(causalGraph);
            session.Complete();

            await _repository.UpdateAsync(session, ct);

            foreach (var domainEvent in session.DomainEvents)
            {
                await _mediator.Publish(domainEvent, ct);
            }

            session.ClearDomainEvents();

            _logger.LogInformation(
                "Diagnostic session {SessionId} completed with {Count} diagnoses",
                session.SessionId, diagnoses.Count);

            return session.SessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Diagnostic session {SessionId} failed for scan {ScanId}",
                session.SessionId, scanId);

            session.MarkFailed();
            await _repository.UpdateAsync(session, ct);
            throw;
        }
    }
}
