using ClearEyeQ.Diagnostic.Application.Interfaces;
using ClearEyeQ.Diagnostic.Domain.Entities;
using ClearEyeQ.Diagnostic.Domain.Enums;
using ClearEyeQ.Diagnostic.Domain.ValueObjects;
using ClearEyeQ.Diagnostic.Infrastructure.ML.Proto;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.Diagnostic.Infrastructure.ML;

public sealed class GrpcDiagnosticMLClient : IDiagnosticMLClient
{
    private readonly DiagnosticMLService.DiagnosticMLServiceClient _client;
    private readonly ILogger<GrpcDiagnosticMLClient> _logger;

    public GrpcDiagnosticMLClient(
        DiagnosticMLService.DiagnosticMLServiceClient client,
        ILogger<GrpcDiagnosticMLClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<List<Diagnosis>> ClassifyConditionsAsync(
        ScanId scanId,
        double rednessScore,
        Dictionary<string, double> zoneScores,
        double tearFilmBreakUpTime,
        List<EnvironmentalFactorInput> environmentalFactors,
        List<MonitoringMetricInput> monitoringMetrics,
        CancellationToken ct)
    {
        var request = new ClassificationRequest
        {
            ScanId = scanId.Value.ToString(),
            RednessScore = rednessScore,
            TearFilmBreakUpTime = tearFilmBreakUpTime
        };

        foreach (var (key, value) in zoneScores)
        {
            request.ZoneScores.Add(key, value);
        }

        foreach (var ef in environmentalFactors)
        {
            request.EnvironmentalFactors.Add(new EnvironmentalFactor
            {
                FactorType = ef.FactorType,
                Value = ef.Value,
                Timestamp = ef.Timestamp
            });
        }

        foreach (var mm in monitoringMetrics)
        {
            request.MonitoringMetrics.Add(new MonitoringMetric
            {
                MetricType = mm.MetricType,
                Value = mm.Value,
                Source = mm.Source
            });
        }

        _logger.LogInformation("Calling ClassifyConditions for scan {ScanId}", scanId);

        var response = await _client.ClassifyConditionsAsync(request, cancellationToken: ct);

        return response.Diagnoses.Select(d => new Diagnosis(
            d.ConditionCode,
            d.ConditionName,
            new ConfidenceScore(d.Confidence),
            ParseSeverity(d.Severity),
            d.EvidenceKeys.Select(k => new EvidenceReference("ML", k, $"Evidence from ML classification: {k}")).ToList()
        )).ToList();
    }

    public async Task<CausalGraph> InferCausalGraphAsync(
        ScanId scanId,
        List<Diagnosis> diagnoses,
        List<EnvironmentalFactorInput> environmentalFactors,
        List<MonitoringMetricInput> monitoringMetrics,
        List<string> medications,
        CancellationToken ct)
    {
        var request = new CausalGraphRequest
        {
            ScanId = scanId.Value.ToString()
        };

        foreach (var d in diagnoses)
        {
            request.Diagnoses.Add(new DiagnosisResult
            {
                ConditionCode = d.ConditionCode,
                ConditionName = d.ConditionName,
                Confidence = d.ConfidenceScore.Value,
                Severity = d.Severity.ToString()
            });
        }

        foreach (var ef in environmentalFactors)
        {
            request.EnvironmentalFactors.Add(new EnvironmentalFactor
            {
                FactorType = ef.FactorType,
                Value = ef.Value,
                Timestamp = ef.Timestamp
            });
        }

        foreach (var mm in monitoringMetrics)
        {
            request.MonitoringMetrics.Add(new MonitoringMetric
            {
                MetricType = mm.MetricType,
                Value = mm.Value,
                Source = mm.Source
            });
        }

        request.Medications.AddRange(medications);

        _logger.LogInformation("Calling InferCausalGraph for scan {ScanId}", scanId);

        var response = await _client.InferCausalGraphAsync(request, cancellationToken: ct);

        var nodes = response.Nodes.Select(n => new CausalFactor(
            n.NodeId,
            n.Label,
            ParseCausalCategory(n.Category),
            n.Weight)).ToList();

        var edges = response.Edges.Select(e => new CausalRelation(
            e.SourceId, e.TargetId, e.Strength)).ToList();

        return new CausalGraph(nodes, edges);
    }

    private static Severity ParseSeverity(string severity)
    {
        return Enum.TryParse<Severity>(severity, ignoreCase: true, out var result)
            ? result
            : Severity.None;
    }

    private static CausalCategory ParseCausalCategory(string category)
    {
        return Enum.TryParse<CausalCategory>(category, ignoreCase: true, out var result)
            ? result
            : CausalCategory.Environmental;
    }
}
