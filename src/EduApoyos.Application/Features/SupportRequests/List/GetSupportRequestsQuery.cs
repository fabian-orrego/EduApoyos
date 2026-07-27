using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.List;

/// <summary>
/// Paginated listing of support requests. Advisors receive the global catalog (US-015);
/// students are automatically scoped to their own requests by the handler.
/// All filters are optional; when they are <c>null</c> no filter is applied for that field.
/// </summary>
public sealed record GetSupportRequestsQuery(
    int PageNumber,
    int PageSize,
    SupportRequestStatus? Status,
    SupportType? SupportType,
    DateTime? FromDate,
    DateTime? ToDate) : IRequest<Result<PagedResult<SupportRequestListItem>>>;
