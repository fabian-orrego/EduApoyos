using EduApoyos.Application.Common.Identity;

namespace EduApoyos.Api.Configuration;

/// <summary>
/// Registers <see cref="ICurrentUserService"/> along with the <see cref="IHttpContextAccessor"/>
/// it depends on. Kept in the API project so the Application layer stays transport-agnostic.
/// </summary>
internal static class CurrentUserExtensions
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}
