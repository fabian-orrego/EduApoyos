using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.Students.List;

/// <summary>
/// Handles <see cref="GetStudentsQuery"/> (US-011). The heavy lifting (projection + pagination)
/// is delegated to <see cref="IStudentRepository.GetPagedAsync"/> so the handler stays focused
/// on wrapping the outcome with the <see cref="Result"/> pattern.
/// </summary>
public sealed class GetStudentsQueryHandler
    : IRequestHandler<GetStudentsQuery, Result<PagedResult<StudentListItem>>>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentsQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<PagedResult<StudentListItem>>> Handle(
        GetStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await _studentRepository
            .GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(page);
    }
}
