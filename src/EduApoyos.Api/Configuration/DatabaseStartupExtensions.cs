using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Api.Configuration;

internal static class DatabaseStartupExtensions
{
    /// <summary>
    /// Applies any pending EF Core migrations at startup. Only enabled in Development to satisfy
    /// </summary>
    public static async Task ApplyPendingMigrationsAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("EduApoyos.Api.DatabaseStartup");

        try
        {
            logger.LogInformation("Applying pending EF Core migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("EF Core migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply EF Core migrations at startup.");
            throw;
        }
    }
}
