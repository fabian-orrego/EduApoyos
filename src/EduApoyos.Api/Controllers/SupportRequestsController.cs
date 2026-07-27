using EduApoyos.Api.Configuration;
using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Features.SupportRequests.Certificate;
using EduApoyos.Application.Features.SupportRequests.ChangeStatus;
using EduApoyos.Application.Features.SupportRequests.Create;
using EduApoyos.Application.Features.SupportRequests.Detail;
using EduApoyos.Application.Features.SupportRequests.List;
using EduApoyos.Application.Features.SupportRequests.Update;
using EduApoyos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Endpoints related to the SupportRequest aggregate (US-013 → US-018).
/// </summary>
[ApiController]
[Route("api/solicitudes")]
[Produces("application/json")]
[Authorize]
public sealed class SupportRequestsController : ControllerBase
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 10;

    private readonly ISender _sender;

    public SupportRequestsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registers a new support request on behalf of a student (US-013). Callable by
    /// authenticated Advisors or Students.
    /// </summary>
    /// <response code="201">The request was created successfully.</response>
    /// <response code="400">The request body is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not authorized for this operation.</response>
    /// <response code="404">The referenced student does not exist.</response>
    [HttpPost]
    [Authorize(Roles = "Advisor,Student")]
    [ProducesResponseType(typeof(CreateSupportRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateSupportRequestRequest request,
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
    /// Returns a paginated list of support requests. Advisors see the full catalog with
    /// optional filters (US-015). Students see only their own requests, regardless of who
    /// created them or the current status (student portal).
    /// </summary>
    /// <param name="pageNumber">One-based page index. Defaults to 1.</param>
    /// <param name="pageSize">Number of records per page. Capped at 100.</param>
    /// <param name="status">Optional status filter (1 = Pending, 2 = UnderReview, 3 = Approved, 4 = Rejected).</param>
    /// <param name="supportType">Optional support type filter (1 = Scholarship, 2 = Loan, 3 = Subsidy).</param>
    /// <param name="fromDate">Optional inclusive lower bound of the request date.</param>
    /// <param name="toDate">Optional inclusive upper bound of the request date.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [Authorize(Roles = "Advisor,Student")]
    [ProducesResponseType(typeof(PagedResult<SupportRequestListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] int pageNumber = DefaultPageNumber,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] SupportRequestStatus? status = null,
        [FromQuery] SupportType? supportType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSupportRequestsQuery(
            pageNumber,
            pageSize,
            status,
            supportType,
            fromDate,
            toDate);

        var result = await _sender.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.ToProblem(HttpContext);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the full detail of a support request including its status history (US-014).
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Advisor,Student")]
    [ProducesResponseType(typeof(SupportRequestDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetSupportRequestByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.ToProblem(HttpContext);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates the editable business fields of a support request (US-016 nota #1). Only
    /// available while the request has not been approved or rejected.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Advisor")]
    [ProducesResponseType(typeof(UpdateSupportRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateSupportRequestRequest request,
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
    /// Applies a status transition to a support request (US-016). Only the transitions defined
    /// by the state machine are accepted.
    /// </summary>
    [HttpPatch("{id:guid}/estado")]
    [Authorize(Roles = "Advisor")]
    [ProducesResponseType(typeof(ChangeSupportRequestStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatusAsync(
        [FromRoute] Guid id,
        [FromBody] ChangeSupportRequestStatusRequest request,
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
    /// Generates and downloads the PDF constancia of the request (US-018). Only the owning
    /// student may retrieve the document. The response is marked as non-cacheable so the
    /// browser always receives a document generated from the current database state.
    /// </summary>
    [HttpGet("{id:guid}/constancia")]
    [Authorize(Roles = "Student")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadCertificateAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GenerateSupportRequestCertificateQuery(id);
        var result = await _sender.Send(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error.ToProblem(HttpContext);
        }

        var certificate = result.Value;
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        Response.Headers.Pragma = "no-cache";
        return File(certificate.Content, certificate.ContentType, certificate.FileName);
    }
}
