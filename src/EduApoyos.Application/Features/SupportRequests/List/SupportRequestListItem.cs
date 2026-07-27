namespace EduApoyos.Application.Features.SupportRequests.List;

/// <summary>
/// Row projection returned by <see cref="GetSupportRequestsQuery"/> (US-015). The advisor grid
/// needs to identify the student by name and document, plus the request metadata (type,
/// status, amount, date). Enum values are exposed as integers so the API contract stays
/// language-agnostic (see <see cref="Domain.Enums.SupportType"/> and
/// <see cref="Domain.Enums.SupportRequestStatus"/>).
/// </summary>
public sealed record SupportRequestListItem(
    Guid Id,
    string StudentFullName,
    string StudentDocumentNumber,
    int SupportType,
    int Status,
    decimal RequestedAmount,
    DateTime RequestedAt);
