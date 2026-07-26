using Microsoft.Extensions.DependencyInjection;

namespace EduApoyos.Api.Configuration;

internal static class ClientCorsExtensions
{
    public const string ClientPolicyName = "EduApoyosClient";

    /// <summary>
    /// Allows the Angular dev server (<c>http://localhost:4200</c>) to talk directly to the API
    /// during development. Origins can be overridden via <c>Cors:AllowedOrigins</c> in
    /// configuration when the frontend is served from a different host.
    /// </summary>
    public static IServiceCollection AddClientCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? new[]
            {
                "http://localhost:4200",
                "https://localhost:4200",
            };

        services.AddCors(options =>
        {
            options.AddPolicy(ClientPolicyName, policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        return services;
    }
}
