using EduApoyos.Application.Common.Identity;
using EduApoyos.Infrastructure.Authentication;
using EduApoyos.Infrastructure.Identity;
using EduApoyos.Infrastructure.Persistence;
using EduApoyos.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduApoyos.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultConnectionName = "DefaultConnection";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnectionName)
            ?? throw new InvalidOperationException(
                $"Connection string '{DefaultConnectionName}' is not configured.");

        services.AddSingleton<UtcDateTimeSaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(sp.GetRequiredService<UtcDateTimeSaveChangesInterceptor>());
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();

        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(
                s => !string.IsNullOrWhiteSpace(s.SecretKey),
                "The 'Jwt:SecretKey' setting must be configured.")
            .Validate(
                s => !string.IsNullOrWhiteSpace(s.Issuer),
                "The 'Jwt:Issuer' setting must be configured.")
            .Validate(
                s => !string.IsNullOrWhiteSpace(s.Audience),
                "The 'Jwt:Audience' setting must be configured.")
            .Validate(
                s => s.AccessTokenExpirationMinutes > 0,
                "The 'Jwt:AccessTokenExpirationMinutes' setting must be positive.")
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
