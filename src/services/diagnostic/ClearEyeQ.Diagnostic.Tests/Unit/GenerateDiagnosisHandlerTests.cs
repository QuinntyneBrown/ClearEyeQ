using ClearEyeQ.Diagnostic.Application.Commands.GenerateDiagnosis;
using ClearEyeQ.Diagnostic.Application.Interfaces;
using ClearEyeQ.Diagnostic.Domain.Aggregates;
using ClearEyeQ.Diagnostic.Domain.Entities;
using ClearEyeQ.Diagnostic.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ClearEyeQ.Diagnostic.Tests.Unit;

public sealed class GenerateDiagnosisHandlerTests
{
    private readonly IDiagnosticSessionRepository _repository;
    private readonly IDiagnosticMLClient _mlClient;
    private readonly IMedicationRepository _medicationRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateDiagnosisHandler> _logger;
    private readonly GenerateDiagnosisHandler _handler;

    public GenerateDiagnosisHandlerTests()
    {
        _repository = Substitute.For<IDiagnosticSessionRepository>();
        _mlClient = Substitute.For<IDiagnosticMLClient>();
        _medicationRepository = Substitute.For<IMedicationRepository>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<GenerateDiagnosisHandler>>();

        _handler = new GenerateDiagnosisHandler(
            _repository, _mlClient, _medicationRepository, _mediator, _logger);
    }

    [Fact]
    public async Task Handle_ShouldCreateSessionAndReturnId()
    {
        var command = new GenerateDiagnosisCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _medicationRepository
            .GetActiveMedicationsAsync(Arg.Any<UserId>(), Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns(new List<string> { "Aspirin" });

        _mlClient
            .ClassifyConditionsAsync(
                Arg.Any<ScanId>(), Arg.Any<double>(), Arg.Any<Dictionary<string, double>>(),
                Arg.Any<double>(), Arg.Any<List<EnvironmentalFactorInput>>(),
                Arg.Any<List<MonitoringMetricInput>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Diagnosis>
            {
                new("H10.1", "Allergic Conjunctivitis", new ConfidenceScore(0.85), Severity.Moderate,
                    [new EvidenceReference("Scan", "zone-1", "Redness detected")])
            });

        _mlClient
            .InferCausalGraphAsync(
                Arg.Any<ScanId>(), Arg.Any<List<Diagnosis>>(),
                Arg.Any<List<EnvironmentalFactorInput>>(), Arg.Any<List<MonitoringMetricInput>>(),
                Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(new CausalGraph(
                [new CausalFactor("f1", "Pollen", Domain.Enums.CausalCategory.Environmental, 0.8)],
                [new CausalRelation("f1", "d1", 0.9)]));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
        await _repository.Received(1).AddAsync(Arg.Any<DiagnosticSession>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(Arg.Any<DiagnosticSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMLClientFails_ShouldMarkSessionFailed()
    {
        var command = new GenerateDiagnosisCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _medicationRepository
            .GetActiveMedicationsAsync(Arg.Any<UserId>(), Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        _mlClient
            .ClassifyConditionsAsync(
                Arg.Any<ScanId>(), Arg.Any<double>(), Arg.Any<Dictionary<string, double>>(),
                Arg.Any<double>(), Arg.Any<List<EnvironmentalFactorInput>>(),
                Arg.Any<List<MonitoringMetricInput>>(), Arg.Any<CancellationToken>())
            .Returns<List<Diagnosis>>(x => throw new InvalidOperationException("ML service unavailable"));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _repository.Received(1).UpdateAsync(
            Arg.Is<DiagnosticSession>(s => s.Status == Domain.Enums.DiagnosticStatus.Failed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishDomainEvents()
    {
        var command = new GenerateDiagnosisCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _medicationRepository
            .GetActiveMedicationsAsync(Arg.Any<UserId>(), Arg.Any<TenantId>(), Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        _mlClient
            .ClassifyConditionsAsync(
                Arg.Any<ScanId>(), Arg.Any<double>(), Arg.Any<Dictionary<string, double>>(),
                Arg.Any<double>(), Arg.Any<List<EnvironmentalFactorInput>>(),
                Arg.Any<List<MonitoringMetricInput>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Diagnosis>
            {
                new("H10.1", "Allergic Conjunctivitis", new ConfidenceScore(0.9), Severity.Moderate,
                    [new EvidenceReference("Scan", "zone-1", "Redness")])
            });

        _mlClient
            .InferCausalGraphAsync(
                Arg.Any<ScanId>(), Arg.Any<List<Diagnosis>>(),
                Arg.Any<List<EnvironmentalFactorInput>>(), Arg.Any<List<MonitoringMetricInput>>(),
                Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(new CausalGraph([], []));

        await _handler.Handle(command, CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Any<Domain.Events.DiagnosisCompletedEvent>(),
            Arg.Any<CancellationToken>());
    }
}
