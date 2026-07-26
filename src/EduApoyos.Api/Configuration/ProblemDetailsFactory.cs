using EduApoyos.Application.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Configuration;

/// <summary>
/// Bridges the Application layer <see cref="Result"/> pattern with ASP.NET Core action results.
/// Keeps the mapping between <see cref="ErrorType"/> and HTTP status codes in a single place.
/// </summary>
internal static class ResultActionResultExtensions
{
    public static IActionResult ToProblem(this Error error, HttpContext context)
    {
        var (statusCode, title) = MapError(error);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = error.Message,
            Instance = context.Request.Path,
            Type = TypeUri(statusCode),
        };

        problem.Extensions["code"] = error.Code;

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" },
        };
    }

    private static (int StatusCode, string Title) MapError(Error error) => error.Type switch
    {
        ErrorType.Validation => (StatusCodes.Status400BadRequest, "Solicitud inválida."),
        ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso no encontrado."),
        ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflicto con el estado actual del recurso."),
        ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "No autenticado."),
        ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Acceso denegado."),
        _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor."),
    };

    private static string TypeUri(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    };
}
