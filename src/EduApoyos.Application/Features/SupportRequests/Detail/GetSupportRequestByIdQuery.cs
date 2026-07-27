using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.Detail;

/// <summary>
/// Retrieves the full detail of a support request (US-014). Access rules:
/// <list type="bullet">
///   <item>Advisors can consult any request.</item>
///   <item>Students can only consult their own requests (403 otherwise).</item>
/// </list>
/// </summary>
public sealed record GetSupportRequestByIdQuery(Guid Id)
    : IRequest<Result<SupportRequestDetail>>;
