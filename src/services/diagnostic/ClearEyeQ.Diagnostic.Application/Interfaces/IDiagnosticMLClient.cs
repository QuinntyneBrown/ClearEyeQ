using ClearEyeQ.Diagnostic.Domain.Entities;
using ClearEyeQ.Diagnostic.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Diagnostic.Application.Interfaces;

public interface IDiagnosticMLClient
{
    Task<List<Diagnosis>> ClassifyConditionsAsync(
        ScanId scanId,
        double rednessScore,
        Dictionary<string, double> zoneScores,
        double tearFilmBreakUpTime,
        List<EnvironmentalFactorInput> environmentalFactors,
        List<MonitoringMetricInput> monitoringMetrics,
        CancellationToken ct);

    Task<CausalGraph> InferCausalGraphAsync(
        ScanId scanId,
        List<Diagnosis> diagnoses,
        List<EnvironmentalFactorInput> environmentalFactors,
        List<MonitoringMetricInput> monitoringMetrics,
        List<string> medications,
        CancellationToken ct);
}

public sealed record EnvironmentalFactorInput(string FactorType, double Value, string Timestamp);
public sealed record MonitoringMetricInput(string MetricType, double Value, string Source);
