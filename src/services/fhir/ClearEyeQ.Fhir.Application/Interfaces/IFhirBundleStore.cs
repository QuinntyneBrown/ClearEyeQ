namespace ClearEyeQ.Fhir.Application.Interfaces;

/// <summary>
/// Stores FHIR export bundles in persistent storage.
/// </summary>
public interface IFhirBundleStore
{
    Task<string> StoreAsync(Guid tenantId, Guid patientId, string bundleJson, CancellationToken cancellationToken = default);
    Task<string?> RetrieveAsync(Guid tenantId, string blobName, CancellationToken cancellationToken = default);
}
