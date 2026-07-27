namespace EduApoyos.Application.Features.SupportRequests.Update;

/// <summary>
/// Public representation of a support request after a successful details update (US-016).
/// Enum values are exposed as integers so the API contract stays language-agnostic.
/// </summary>
public sealed record UpdateSupportRequestResponse(
    Guid Id,
    Guid StudentId,
    int SupportType,
    decimal RequestedAmount,
    string Description,
    int Status,
    DateTime RequestedAt,
    DateTime UpdatedAt);
