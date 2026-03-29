using ClearEyeQ.Fhir.Application.Commands.ExportPatientData;
using ClearEyeQ.Fhir.Application.Commands.ImportClinicalData;
using ClearEyeQ.Fhir.Application.Queries.GetFhirObservations;
using ClearEyeQ.Fhir.Application.Queries.GetFhirPatient;
using Hl7.Fhir.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Fhir.API.Controllers;

[ApiController]
[Route("api/fhir")]
[Authorize]
public sealed class FhirController : ControllerBase
{
    private readonly IMediator _mediator;

    public FhirController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("export")]
    [ProducesResponseType(typeof(ExportPatientDataResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromBody] ExportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ExportPatientDataCommand(tenantId, request.PatientId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportClinicalDataResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Import(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromBody] ImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ImportClinicalDataCommand(tenantId, request.BundleJson),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("Patient/{id:guid}")]
    [Produces("application/fhir+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatient(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var patient = await _mediator.Send(new GetFhirPatientQuery(tenantId, id), cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        var serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = true });
        var json = serializer.SerializeToString(patient);
        return Content(json, "application/fhir+json");
    }

    [HttpGet("Observation")]
    [Produces("application/fhir+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetObservations(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromQuery] Guid patient,
        CancellationToken cancellationToken)
    {
        var bundle = await _mediator.Send(new GetFhirObservationsQuery(tenantId, patient), cancellationToken);

        var serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = true });
        var json = serializer.SerializeToString(bundle);
        return Content(json, "application/fhir+json");
    }
}

public sealed record ExportRequest(Guid PatientId);
public sealed record ImportRequest(string BundleJson);
