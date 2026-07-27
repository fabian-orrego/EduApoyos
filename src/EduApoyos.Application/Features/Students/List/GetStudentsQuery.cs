using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.Students.List;

/// <summary>
/// Returns a paginated list of students for the advisor grid (US-011). Pagination follows
/// the project-wide envelope <see cref="PagedResult{T}"/> and is bounded by the maximum page
/// size defined in <see cref="GetStudentsQueryValidator"/>.
/// </summary>
public sealed record GetStudentsQuery(int PageNumber, int PageSize)
    : IRequest<Result<PagedResult<StudentListItem>>>;
