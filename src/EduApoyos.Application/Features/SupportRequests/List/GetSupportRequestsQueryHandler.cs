using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.List;

/// <summary>
/// Handles <see cref="GetSupportRequestsQuery"/>. Advisors receive the full filtered catalog
/// (US-015). Students receive only the requests that belong to their own student profile,
/// regardless of who created them or the current status (student portal / US-014 ownership).
/// </summary>
public sealed class GetSupportRequestsQueryHandler
    : IRequestHandler<GetSupportRequestsQuery, Result<PagedResult<SupportRequestListItem>>>
{
    private readonly ISupportRequestRepository _repository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUserService _currentUser;

    public GetSupportRequestsQueryHandler(
        ISupportRequestRepository repository,
        IStudentRepository studentRepository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _studentRepository = studentRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<SupportRequestListItem>>> Handle(
        GetSupportRequestsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? studentScope = null;

        if (_currentUser.Role == UserRole.Student)
        {
            if (_currentUser.UserId is not Guid userId)
            {
                return Result.Failure<PagedResult<SupportRequestListItem>>(
                    Error.Forbidden(
                        "supportRequests.list.forbidden",
                        "No tienes permisos para consultar solicitudes."));
            }

            studentScope = await _studentRepository
                .GetIdByUserIdAsync(userId, cancellationToken)
                .ConfigureAwait(false);

            // A student account without a linked Student profile cannot own requests; return an
            // empty page instead of leaking the global catalog.
            if (studentScope is null)
            {
                return Result.Success(
                    PagedResult<SupportRequestListItem>.Empty(
                        request.PageNumber,
                        request.PageSize));
            }
        }
        else if (_currentUser.Role != UserRole.Advisor)
        {
            return Result.Failure<PagedResult<SupportRequestListItem>>(
                Error.Forbidden(
                    "supportRequests.list.forbidden",
                    "No tienes permisos para consultar solicitudes."));
        }

        var page = await _repository
            .GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.SupportType,
                request.FromDate,
                request.ToDate,
                studentScope,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(page);
    }
}
