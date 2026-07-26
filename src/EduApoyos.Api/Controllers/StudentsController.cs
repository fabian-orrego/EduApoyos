using EduApoyos.Api.Configuration;
using EduApoyos.Application.Features.Students.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Endpoints related to the Student aggregate.
/// </summary>
[ApiController]
[Route("api/estudiantes")]
[Produces("application/json")]
[Authorize(Roles = "Advisor")]
public sealed class StudentsController : ControllerBase
{
    private readonly ISender _sender;

    public StudentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registers a new student (US-008). Only Advisors may call this endpoint.
    /// </summary>
    /// <param name="request">The registration payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="201">The student was created successfully.</response>
    /// <response code="400">The request body is invalid or the user is not eligible.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have the Advisor role.</response>
    /// <response code="409">The user is already linked to a student or the document is duplicated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateStudentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await _sender.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.ToProblem(HttpContext);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}
