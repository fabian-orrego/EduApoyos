using EduApoyos.Application.Features.SupportRequests.ChangeStatus;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// HTTP payload for <c>PATCH /api/solicitudes/{id}/estado</c> (US-016). The status is sent
/// as the integer value of the <see cref="Domain.Enums.SupportRequestStatus"/> enum
/// (1 = Pending, 2 = UnderReview, 3 = Approved, 4 = Rejected).
/// </summary>
public sealed record ChangeSupportRequestStatusRequest(
    int NewStatus,
    string? Notes)
{
    internal ChangeSupportRequestStatusCommand ToCommand(Guid id) =>
        new(id, (SupportRequestStatus)NewStatus, Notes);
}
