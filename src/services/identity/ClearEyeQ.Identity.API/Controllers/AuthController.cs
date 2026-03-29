using ClearEyeQ.Identity.Application.Commands.Authenticate;
using ClearEyeQ.Identity.Application.Commands.RefreshToken;
using ClearEyeQ.Identity.Application.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearEyeQ.Identity.API.Controllers;

[ApiController]
[Route("api/identity/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _mediator.Send(command, cancellationToken);
            return Created($"api/identity/users/{userId}", new { UserId = userId });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { Error = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] AuthenticateCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        // In a full implementation, this would revoke the user's current refresh token
        // and invalidate the session. For now, the client discards the token.
        return NoContent();
    }
}
