using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Features.SupportRequests.Detail;
using EduApoyos.Application.Features.SupportRequests.List;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Common.Persistence;

/// <summary>
/// Application-level abstraction over the <c>SupportRequests</c> aggregate. Concrete
/// implementations live in <c>EduApoyos.Infrastructure</c> so the Application project stays
/// independent from EF Core.
/// </summary>
public interface ISupportRequestRepository
{
    /// <summary>
    /// Persists a new support request together with the initial history entry (US-013).
    /// The two writes must happen inside the same transaction to keep the aggregate consistent.
    /// </summary>
    Task CreateAsync(
        SupportRequest supportRequest,
        StatusHistory initialHistory,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads a request by identifier with tracking enabled so the caller can mutate it (US-016).
    /// Returns <c>null</c> when the record does not exist.
    /// </summary>
    Task<SupportRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Applies a mutation performed on the aggregate loaded via <see cref="GetByIdAsync"/>.
    /// When a status transition happened the caller passes the corresponding history entry so
    /// both writes are committed atomically (US-016 RN-6).
    /// </summary>
    Task UpdateAsync(
        SupportRequest supportRequest,
        StatusHistory? history,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the full detail projection (with linked student, advisor and history) for a
    /// support request (US-014). Returns <c>null</c> when the record does not exist. The query
    /// uses <c>AsNoTracking()</c> per project conventions.
    /// </summary>
    Task<SupportRequestDetail?> GetDetailAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the identifier of the student the supplied request belongs to. Used by
    /// authorization checks (US-014 RN-1, US-018 RN-1) without loading the full aggregate.
    /// Returns <c>null</c> when the record does not exist.
    /// </summary>
    Task<Guid?> GetStudentIdAsync(Guid supportRequestId, CancellationToken cancellationToken);

    /// <summary>
    /// Paginated list projection for the support-request grids (US-015 + student portal).
    /// Filters are optional; when they are all null the result includes every request that
    /// matches the optional <paramref name="studentId"/> scope.
    /// </summary>
    /// <param name="studentId">
    /// When set, restricts the result to requests belonging to that student so a Student
    /// caller can only see their own records regardless of who created them or the status.
    /// </param>
    Task<PagedResult<SupportRequestListItem>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        SupportRequestStatus? status,
        SupportType? supportType,
        DateTime? fromDate,
        DateTime? toDate,
        Guid? studentId,
        CancellationToken cancellationToken);
}
