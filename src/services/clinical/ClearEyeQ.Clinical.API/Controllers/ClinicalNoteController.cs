using ClearEyeQ.Clinical.Application.Commands.CreateClinicalNote;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Clinical.API.Controllers;

[ApiController]
[Route("api/clinical/patients/{patientId:guid}/notes")]
[Authorize]
public sealed class ClinicalNoteController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClinicalNoteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateNoteResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateNote(
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        Guid patientId,
        [FromBody] CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var noteId = await _mediator.Send(
            new CreateClinicalNoteCommand(tenantId, patientId, request.ClinicianId, request.Content),
            cancellationToken);

        return CreatedAtAction(null, new CreateNoteResponse(noteId));
    }
}

public sealed record CreateNoteRequest(string ClinicianId, string Content);
public sealed record CreateNoteResponse(Guid NoteId);
