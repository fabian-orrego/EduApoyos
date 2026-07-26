using EduApoyos.Api.Configuration;
using EduApoyos.Application.Features.Auth.Login;
using EduApoyos.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Endpoints related to authentication and user account lifecycle.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registers a new user in the platform (US-004). Public endpoint - no JWT is issued.
    /// </summary>
    /// <param name="request">The registration payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="201">The user was created successfully.</response>
    /// <response code="400">The request body is invalid.</response>
    /// <response code="409">The email is already registered.</response>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await _sender.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.ToProblem(HttpContext);
        }

        // There is no GET-by-id endpoint yet (US-004 is registration only), so we return 201 with
        // the created resource in the body but no Location header. CreatedAtAction cannot be used
        // here because it would fail to resolve a route for the target action.
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /// <summary>
    /// Authenticates a user and returns a signed JWT (US-005). Public endpoint.
    /// </summary>
    /// <param name="request">The login payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The credentials are valid and a token has been issued.</response>
    /// <response code="400">The request body is invalid.</response>
    /// <response code="401">The credentials are invalid.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await _sender.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.ToProblem(HttpContext);
        }

        return Ok(result.Value);
    }
}
