using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.ChangeStatus;

/// <summary>
/// Transitions a support request between the states allowed by US-016 (Pending → UnderReview,
/// UnderReview → Approved, UnderReview → Rejected). The advisor is captured from the current
/// user context so the aggregate always has traceability of who performed the change.
/// </summary>
public sealed record ChangeSupportRequestStatusCommand(
    Guid Id,
    SupportRequestStatus NewStatus,
    string? Notes) : IRequest<Result<ChangeSupportRequestStatusResponse>>;
