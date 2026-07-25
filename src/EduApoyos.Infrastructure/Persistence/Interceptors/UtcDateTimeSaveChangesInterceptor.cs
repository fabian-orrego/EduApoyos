using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EduApoyos.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Enforces RN-004: every <see cref="DateTime"/> value written to the database is normalised to
/// UTC before persistence. Unspecified kinds are assumed to already be UTC and are flagged as
/// such; local times are converted.
/// </summary>
public sealed class UtcDateTimeSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        NormalizeDateTimes(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        NormalizeDateTimes(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void NormalizeDateTimes(DbContextEventData eventData)
    {
        if (eventData.Context is null)
        {
            return;
        }

        foreach (EntityEntry entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.State is not (Microsoft.EntityFrameworkCore.EntityState.Added
                or Microsoft.EntityFrameworkCore.EntityState.Modified))
            {
                continue;
            }

            foreach (PropertyEntry property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dateTime)
                {
                    property.CurrentValue = dateTime.Kind switch
                    {
                        DateTimeKind.Utc => dateTime,
                        DateTimeKind.Local => dateTime.ToUniversalTime(),
                        _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                    };
                }
            }
        }
    }
}
