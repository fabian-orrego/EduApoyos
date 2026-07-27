namespace EduApoyos.Application.Features.SupportRequests.ChangeStatus;

/// <summary>
/// Public representation of a support request after a successful status transition (US-016).
/// Includes the identifier of the advisor who performed the change so the client can display
/// the traceability information without an extra round-trip.
/// </summary>
public sealed record ChangeSupportRequestStatusResponse(
    Guid Id,
    int PreviousStatus,
    int NewStatus,
    Guid AdvisorId,
    DateTime UpdatedAt);
