namespace EduApoyos.Application.Features.SupportRequests.Create;

/// <summary>
/// Public representation of a freshly created support request (US-013). Enum values are
/// exposed as integers to keep the API contract language-agnostic (see
/// <see cref="Domain.Enums.SupportType"/> and <see cref="Domain.Enums.SupportRequestStatus"/>).
/// </summary>
public sealed record CreateSupportRequestResponse(
    Guid Id,
    Guid StudentId,
    int SupportType,
    decimal RequestedAmount,
    string Description,
    int Status,
    DateTime RequestedAt);
