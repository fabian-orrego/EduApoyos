using EduApoyos.Api.Configuration;
using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Features.Students.Create;
using EduApoyos.Application.Features.Students.Delete;
using EduApoyos.Application.Features.Students.List;
using EduApoyos.Application.Features.Students.Update;
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
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 10;

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

    /// <summary>
    /// Returns a paginated list of students (US-011). Only Advisors may call this endpoint.
    /// </summary>
    /// <param name="pageNumber">One-based page index. Defaults to 1.</param>
    /// <param name="pageSize">Number of records per page. Capped at 100.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The page of students was returned successfully.</response>
    /// <response code="400">The pagination parameters are invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have the Advisor role.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<StudentListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] int pageNumber = DefaultPageNumber,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentsQuery(pageNumber, pageSize);
        var result = await _sender.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.ToProblem(HttpContext);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates the academic information of an existing student (US-009).
    /// </summary>
    /// <param name="id">Identifier of the student to update.</param>
    /// <param name="request">The updated academic payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The student was updated successfully.</response>
    /// <response code="400">The request body is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have the Advisor role.</response>
    /// <response code="404">The student does not exist.</response>
    /// <response code="409">The document number is already in use by another student.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateStudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand(id);
        var result = await _sender.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.ToProblem(HttpContext);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a student (US-010). A student with associated support requests cannot be removed.
    /// </summary>
    /// <param name="id">Identifier of the student to delete.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="204">The student was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller does not have the Advisor role.</response>
    /// <response code="404">The student does not exist.</response>
    /// <response code="409">The student has associated support requests.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteStudentCommand(id);
        var result = await _sender.Send(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.ToProblem(HttpContext);
        }

        return NoContent();
    }
}
