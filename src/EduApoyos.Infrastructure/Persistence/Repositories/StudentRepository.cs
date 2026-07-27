using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Features.Students.List;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IStudentRepository"/>. Existence and read-only queries use
/// <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/> per project conventions;
/// mutating operations (update/delete) intentionally load the entity with tracking enabled.
/// </summary>
internal sealed class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StudentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        _dbContext.Students
            .AsNoTracking()
            .AnyAsync(s => s.UserId == userId, cancellationToken);

    public Task<bool> ExistsByDocumentAsync(
        DocumentType documentType,
        string documentNumber,
        CancellationToken cancellationToken) =>
        _dbContext.Students
            .AsNoTracking()
            .AnyAsync(
                s => s.DocumentType == documentType && s.DocumentNumber == documentNumber,
                cancellationToken);

    public Task<bool> ExistsByDocumentAsync(
        DocumentType documentType,
        string documentNumber,
        Guid excludeStudentId,
        CancellationToken cancellationToken) =>
        _dbContext.Students
            .AsNoTracking()
            .AnyAsync(
                s => s.Id != excludeStudentId
                    && s.DocumentType == documentType
                    && s.DocumentNumber == documentNumber,
                cancellationToken);

    public async Task CreateAsync(Student student, CancellationToken cancellationToken)
    {
        await _dbContext.Students.AddAsync(student, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task UpdateAsync(Student student, CancellationToken cancellationToken)
    {
        _dbContext.Students.Update(student);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(Student student, CancellationToken cancellationToken)
    {
        _dbContext.Students.Remove(student);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasSupportRequestsAsync(Guid studentId, CancellationToken cancellationToken) =>
        _dbContext.SupportRequests
            .AsNoTracking()
            .AnyAsync(sr => sr.StudentId == studentId, cancellationToken);

    public async Task<PagedResult<StudentListItem>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        // Join with the Identity user table so the projection carries the full name and email
        // the advisor grid needs. The join is expressed via query syntax to keep the projection
        // translatable by EF Core.
        var query =
            from student in _dbContext.Students.AsNoTracking()
            join user in _dbContext.Set<ApplicationUser>().AsNoTracking()
                on student.UserId equals user.Id
            orderby user.FullName
            select new StudentListItem(
                student.Id,
                user.FullName,
                student.DocumentNumber,
                (int)student.DocumentType,
                student.AcademicProgram,
                student.Semester,
                user.Email!);

        var totalItems = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        if (totalItems == 0)
        {
            return PagedResult<StudentListItem>.Empty(pageNumber, pageSize);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return PagedResult<StudentListItem>.Create(items, pageNumber, pageSize, totalItems);
    }
}
