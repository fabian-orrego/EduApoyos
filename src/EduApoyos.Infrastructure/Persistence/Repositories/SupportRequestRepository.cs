using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Features.SupportRequests.Detail;
using EduApoyos.Application.Features.SupportRequests.List;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ISupportRequestRepository"/>. Read-only queries use
/// <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/> per project
/// conventions; mutating operations intentionally load the entity with tracking enabled.
/// </summary>
internal sealed class SupportRequestRepository : ISupportRequestRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SupportRequestRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(
        SupportRequest supportRequest,
        StatusHistory initialHistory,
        CancellationToken cancellationToken)
    {
        await _dbContext.SupportRequests
            .AddAsync(supportRequest, cancellationToken)
            .ConfigureAwait(false);

        await _dbContext.StatusHistories
            .AddAsync(initialHistory, cancellationToken)
            .ConfigureAwait(false);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<SupportRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.SupportRequests
            .FirstOrDefaultAsync(sr => sr.Id == id, cancellationToken);

    public async Task UpdateAsync(
        SupportRequest supportRequest,
        StatusHistory? history,
        CancellationToken cancellationToken)
    {
        _dbContext.SupportRequests.Update(supportRequest);

        if (history is not null)
        {
            await _dbContext.StatusHistories
                .AddAsync(history, cancellationToken)
                .ConfigureAwait(false);
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Guid?> GetStudentIdAsync(
        Guid supportRequestId,
        CancellationToken cancellationToken)
    {
        var studentId = await _dbContext.SupportRequests
            .AsNoTracking()
            .Where(sr => sr.Id == supportRequestId)
            .Select(sr => (Guid?)sr.StudentId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return studentId;
    }

    public async Task<SupportRequestDetail?> GetDetailAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var users = _dbContext.Set<ApplicationUser>().AsNoTracking();
        var students = _dbContext.Students.AsNoTracking();
        var requests = _dbContext.SupportRequests.AsNoTracking();

        var query =
            from request in requests
            where request.Id == id
            join student in students on request.StudentId equals student.Id
            join studentUser in users on student.UserId equals studentUser.Id
            join advisor in users
                on request.AdvisorId equals advisor.Id into advisorJoin
            from advisor in advisorJoin.DefaultIfEmpty()
            select new
            {
                request.Id,
                request.StudentId,
                StudentFullName = studentUser.FullName,
                StudentEmail = studentUser.Email,
                student.DocumentNumber,
                DocumentType = (int)student.DocumentType,
                student.AcademicProgram,
                student.Semester,
                SupportType = (int)request.SupportType,
                request.RequestedAmount,
                request.Description,
                Status = (int)request.Status,
                request.RequestedAt,
                request.UpdatedAt,
                request.AdvisorId,
                AdvisorFullName = advisor != null ? advisor.FullName : null,
            };

        var row = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            return null;
        }

        var historyQuery =
            from entry in _dbContext.StatusHistories.AsNoTracking()
            where entry.SupportRequestId == id
            join user in users on entry.ChangedByUserId equals user.Id
            orderby entry.ChangedAt
            select new SupportRequestHistoryItem(
                entry.Id,
                (int)entry.PreviousStatus,
                (int)entry.NewStatus,
                entry.ChangedAt,
                entry.ChangedByUserId,
                user.FullName,
                entry.Notes);

        var history = await historyQuery
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new SupportRequestDetail(
            row.Id,
            row.StudentId,
            row.StudentFullName,
            row.StudentEmail ?? string.Empty,
            row.DocumentNumber,
            row.DocumentType,
            row.AcademicProgram,
            row.Semester,
            row.SupportType,
            row.RequestedAmount,
            row.Description,
            row.Status,
            row.RequestedAt,
            row.UpdatedAt,
            row.AdvisorId,
            row.AdvisorFullName,
            history);
    }

    public async Task<PagedResult<SupportRequestListItem>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        SupportRequestStatus? status,
        SupportType? supportType,
        DateTime? fromDate,
        DateTime? toDate,
        Guid? studentId,
        CancellationToken cancellationToken)
    {
        var users = _dbContext.Set<ApplicationUser>().AsNoTracking();
        var students = _dbContext.Students.AsNoTracking();
        var requests = _dbContext.SupportRequests.AsNoTracking();

        if (studentId.HasValue)
        {
            requests = requests.Where(sr => sr.StudentId == studentId.Value);
        }

        if (status.HasValue)
        {
            requests = requests.Where(sr => sr.Status == status.Value);
        }

        if (supportType.HasValue)
        {
            requests = requests.Where(sr => sr.SupportType == supportType.Value);
        }

        if (fromDate.HasValue)
        {
            var normalizedFrom = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            requests = requests.Where(sr => sr.RequestedAt >= normalizedFrom);
        }

        if (toDate.HasValue)
        {
            // Include the whole "to" day by taking the exclusive upper bound at 00:00 of the next day.
            var normalizedTo = DateTime.SpecifyKind(
                toDate.Value.Date.AddDays(1),
                DateTimeKind.Utc);
            requests = requests.Where(sr => sr.RequestedAt < normalizedTo);
        }

        var projection =
            from request in requests
            join student in students on request.StudentId equals student.Id
            join user in users on student.UserId equals user.Id
            orderby request.RequestedAt descending
            select new SupportRequestListItem(
                request.Id,
                user.FullName,
                student.DocumentNumber,
                (int)request.SupportType,
                (int)request.Status,
                request.RequestedAmount,
                request.RequestedAt);

        var totalItems = await projection.CountAsync(cancellationToken).ConfigureAwait(false);
        if (totalItems == 0)
        {
            return PagedResult<SupportRequestListItem>.Empty(pageNumber, pageSize);
        }

        var items = await projection
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return PagedResult<SupportRequestListItem>.Create(items, pageNumber, pageSize, totalItems);
    }
}
