using EduApoyos.Application.Common.Pagination;
using EduApoyos.Application.Features.Students.List;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Common.Persistence;

/// <summary>
/// Application abstraction over the <c>Students</c> table. The concrete implementation lives in
/// <c>EduApoyos.Infrastructure</c> so this project stays independent from EF Core.
/// </summary>
public interface IStudentRepository
{
    /// <summary>
    /// Checks whether the supplied user is already linked to a <see cref="Student"/>. Used to
    /// enforce RN-003 (a user can only be associated to a single student).
    /// </summary>
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a student with the supplied document type + number already exists. Used to
    /// enforce RN-004 (document number must be unique).
    /// </summary>
    Task<bool> ExistsByDocumentAsync(
        DocumentType documentType,
        string documentNumber,
        CancellationToken cancellationToken);

    /// <summary>
    /// Overload of <see cref="ExistsByDocumentAsync(DocumentType, string, CancellationToken)"/>
    /// used during updates (US-009): the record being modified is excluded from the uniqueness
    /// check so keeping the same document does not trigger a false positive.
    /// </summary>
    Task<bool> ExistsByDocumentAsync(
        DocumentType documentType,
        string documentNumber,
        Guid excludeStudentId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists a new <see cref="Student"/> aggregate.
    /// </summary>
    Task CreateAsync(Student student, CancellationToken cancellationToken);

    /// <summary>
    /// Loads a student by its identifier. Returns <c>null</c> when the record does not exist so
    /// callers can decide whether the miss is a not-found or a validation error (US-009, US-010).
    /// The returned entity is tracked because it is loaded to be mutated by the caller.
    /// </summary>
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Applies the changes on a previously loaded <see cref="Student"/> aggregate (US-009).
    /// </summary>
    Task UpdateAsync(Student student, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the supplied <see cref="Student"/> aggregate (US-010).
    /// </summary>
    Task DeleteAsync(Student student, CancellationToken cancellationToken);

    /// <summary>
    /// Returns <c>true</c> when the student has at least one associated support request.
    /// Used to enforce US-010 RN-1 (a student with support requests cannot be deleted).
    /// </summary>
    Task<bool> HasSupportRequestsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a paginated projection of students joined with their Identity user (US-011).
    /// The query is read-only and must use <c>AsNoTracking()</c>.
    /// </summary>
    Task<PagedResult<StudentListItem>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
