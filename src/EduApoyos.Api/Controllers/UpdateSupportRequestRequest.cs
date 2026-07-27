using EduApoyos.Application.Features.SupportRequests.Update;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// HTTP payload for <c>PUT /api/solicitudes/{id}</c> (US-016 nota #1). Kept separate from the
/// MediatR command so the transport model can evolve independently.
/// </summary>
public sealed record UpdateSupportRequestRequest(
    int SupportType,
    decimal RequestedAmount,
    string Description)
{
    internal UpdateSupportRequestCommand ToCommand(Guid id) =>
        new(id, (SupportType)SupportType, RequestedAmount, Description);
}
