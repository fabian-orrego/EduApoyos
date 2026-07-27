namespace EduApoyos.Application.Features.SupportRequests.Detail;

/// <summary>
/// Full projection returned by <see cref="GetSupportRequestByIdQuery"/> (US-014). The detail
/// screen renders the request metadata, the linked student and the ordered status history.
/// Enum values are exposed as integers so the API contract stays language agnostic.
/// </summary>
public sealed record SupportRequestDetail(
    Guid Id,
    Guid StudentId,
    string StudentFullName,
    string StudentEmail,
    string StudentDocumentNumber,
    int StudentDocumentType,
    string StudentAcademicProgram,
    int StudentSemester,
    int SupportType,
    decimal RequestedAmount,
    string Description,
    int Status,
    DateTime RequestedAt,
    DateTime UpdatedAt,
    Guid? AdvisorId,
    string? AdvisorFullName,
    IReadOnlyList<SupportRequestHistoryItem> History);

/// <summary>
/// Single record of the status history timeline for a support request (US-014). The list is
/// returned chronologically ordered by <see cref="ChangedAt"/>.
/// </summary>
public sealed record SupportRequestHistoryItem(
    Guid Id,
    int PreviousStatus,
    int NewStatus,
    DateTime ChangedAt,
    Guid ChangedByUserId,
    string ChangedByFullName,
    string? Notes);
