using System.Reflection;
using Microsoft.OpenApi.Models;

namespace EduApoyos.Api.Configuration;

internal static class SwaggerServiceCollectionExtensions
{
    private const string ApiTitle = "EduApoyos API";
    private const string ApiVersion = "v1";
    private const string JwtSecuritySchemeId = "Bearer";

    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(ApiVersion, new OpenApiInfo
            {
                Title = ApiTitle,
                Version = ApiVersion,
                Description = "API de EduApoyos para la gestión de estudiantes y solicitudes de apoyo.",
                Contact = new OpenApiContact
                {
                    Name = "Equipo EduApoyos",
                },
            });

            options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
            options.SupportNonNullableReferenceTypes();
            options.UseAllOfToExtendReferenceSchemas();

            IncludeXmlComments(options);
            AddJwtBearerSecurity(options);
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{ApiVersion}/swagger.json", $"{ApiTitle} {ApiVersion}");
            options.DocumentTitle = ApiTitle;
            options.DisplayRequestDuration();
        });

        return app;
    }

    private static void IncludeXmlComments(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
    }

    private static void AddJwtBearerSecurity(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Introduce el token JWT en el formato: Bearer {token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = JwtSecuritySchemeId,
            },
        };

        options.AddSecurityDefinition(JwtSecuritySchemeId, securityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [securityScheme] = Array.Empty<string>(),
        });
    }
}
