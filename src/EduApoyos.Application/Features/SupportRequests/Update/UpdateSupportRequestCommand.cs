using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.Update;

/// <summary>
/// Updates the editable business fields of a support request (US-016 nota #1). The command is
/// only valid while the request has not reached a terminal state
/// (<see cref="SupportRequestStatus.Approved"/> or <see cref="SupportRequestStatus.Rejected"/>).
/// </summary>
public sealed record UpdateSupportRequestCommand(
    Guid Id,
    SupportType SupportType,
    decimal RequestedAmount,
    string Description) : IRequest<Result<UpdateSupportRequestResponse>>;
