using EduApoyos.Application.Common.Persistence;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IStudentRepository"/>. Queries use
/// <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/> because they exist only
/// to enforce uniqueness rules and are never used to mutate the returned rows.
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

    public async Task CreateAsync(Student student, CancellationToken cancellationToken)
    {
        await _dbContext.Students.AddAsync(student, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
