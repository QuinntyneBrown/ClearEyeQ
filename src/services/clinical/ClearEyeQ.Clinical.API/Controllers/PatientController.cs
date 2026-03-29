using ClearEyeQ.Clinical.Application.Queries.GetPatientDetail;
using ClearEyeQ.Clinical.Application.Queries.GetPatientList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Clinical.API.Controllers;

[ApiController]
[Route("api/clinical/patients")]
[Authorize]
public sealed class PatientController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PatientSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatients([FromHeader(Name = "X-Tenant-Id")] Guid tenantId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPatientListQuery(tenantId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientDetail(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPatientDetailQuery(tenantId, id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
