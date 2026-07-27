using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.Create;

/// <summary>
/// Registers a new support request on behalf of the student identified by
/// <see cref="StudentEmail"/> (US-013). The email is used as the natural key so the caller
/// (advisor or the student itself) can register the request without knowing the internal
/// student id.
/// </summary>
public sealed record CreateSupportRequestCommand(
    string StudentEmail,
    SupportType SupportType,
    decimal RequestedAmount,
    string Description) : IRequest<Result<CreateSupportRequestResponse>>;
