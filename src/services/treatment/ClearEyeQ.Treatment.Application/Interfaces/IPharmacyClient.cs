namespace ClearEyeQ.Treatment.Application.Interfaces;

public interface IPharmacyClient
{
    Task<PharmacySuggestion?> GetFormulationSuggestionAsync(
        string drugName,
        string currentDosage,
        CancellationToken ct);
}

public sealed record PharmacySuggestion(
    string FormulationName,
    string RecommendedDosage,
    string Route,
    string Rationale);
