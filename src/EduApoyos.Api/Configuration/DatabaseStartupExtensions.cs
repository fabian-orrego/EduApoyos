using EduApoyos.Infrastructure.Persistence;
using EduApoyos.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Api.Configuration;

internal static class DatabaseStartupExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations and, when the database is empty of demo users,
    /// seeds the Advisor / Students / SupportRequests catalog used for local testing.
    /// </summary>
    public static async Task ApplyPendingMigrationsAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("EduApoyos.Api.DatabaseStartup");

        try
        {
            logger.LogInformation("Applying pending EF Core migrations...");
            await context.Database.MigrateAsync().ConfigureAwait(false);
            logger.LogInformation("EF Core migrations applied successfully.");

            await DemoDataSeeder.SeedAsync(services, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply EF Core migrations or seed demo data at startup.");
            throw;
        }
    }
}
