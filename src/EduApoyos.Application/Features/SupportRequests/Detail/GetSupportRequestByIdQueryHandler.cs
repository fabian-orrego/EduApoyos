using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.Detail;

/// <summary>
/// Handles <see cref="GetSupportRequestByIdQuery"/> (US-014). The heavy lifting (projection,
/// history join, <c>AsNoTracking</c>) is delegated to
/// <see cref="ISupportRequestRepository.GetDetailAsync"/> so this handler only orchestrates
/// authorization (RN-1) and error mapping.
/// </summary>
public sealed class GetSupportRequestByIdQueryHandler
    : IRequestHandler<GetSupportRequestByIdQuery, Result<SupportRequestDetail>>
{
    private readonly ISupportRequestRepository _supportRequestRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUserService _currentUser;

    public GetSupportRequestByIdQueryHandler(
        ISupportRequestRepository supportRequestRepository,
        IStudentRepository studentRepository,
        ICurrentUserService currentUser)
    {
        _supportRequestRepository = supportRequestRepository;
        _studentRepository = studentRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<SupportRequestDetail>> Handle(
        GetSupportRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var detail = await _supportRequestRepository
            .GetDetailAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (detail is null)
        {
            return Result.Failure<SupportRequestDetail>(
                Error.NotFound(
                    "supportRequests.notFound",
                    "La solicitud no existe."));
        }

        if (_currentUser.Role == UserRole.Student)
        {
            var callerStudentId = _currentUser.UserId is Guid userId
                ? await _studentRepository
                    .GetIdByUserIdAsync(userId, cancellationToken)
                    .ConfigureAwait(false)
                : null;

            if (callerStudentId is null || callerStudentId.Value != detail.StudentId)
            {
                return Result.Failure<SupportRequestDetail>(
                    Error.Forbidden(
                        "supportRequests.forbidden",
                        "No tienes permisos para consultar esta solicitud."));
            }
        }

        return Result.Success(detail);
    }
}
