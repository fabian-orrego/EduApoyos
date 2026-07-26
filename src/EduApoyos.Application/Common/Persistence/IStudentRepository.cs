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
    /// Persists a new <see cref="Student"/> aggregate.
    /// </summary>
    Task CreateAsync(Student student, CancellationToken cancellationToken);
}
