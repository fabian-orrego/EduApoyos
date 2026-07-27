using EduApoyos.Application.Features.SupportRequests.Create;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// HTTP payload for <c>POST /api/solicitudes</c> (US-013). Kept separate from the MediatR
/// command so the transport model can evolve independently. <see cref="SupportType"/> is sent
/// as the integer value of the <see cref="Domain.Enums.SupportType"/> enum
/// (1 = Scholarship, 2 = Loan, 3 = Subsidy).
/// </summary>
public sealed record CreateSupportRequestRequest(
    string StudentEmail,
    int SupportType,
    decimal RequestedAmount,
    string Description)
{
    internal CreateSupportRequestCommand ToCommand() =>
        new(StudentEmail, (SupportType)SupportType, RequestedAmount, Description);
}
