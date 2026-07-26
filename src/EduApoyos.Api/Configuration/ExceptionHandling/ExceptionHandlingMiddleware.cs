using System.Net;
using System.Net.Mime;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Configuration.ExceptionHandling;

/// <summary>
/// Global exception filter that converts thrown exceptions <see cref="ProblemDetails"/>.
/// Validation failures produced by the MediatR pipeline are surfaced as HTTP 400 with an
/// <see cref="ValidationProblemDetails"/> payload.
/// </summary>
internal sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (ValidationException validationException)
        {
            await WriteValidationProblemAsync(context, validationException).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);
            await WriteProblemAsync(
                context,
                statusCode: (int)HttpStatusCode.InternalServerError,
                title: "Se produjo un error inesperado.",
                detail: exception.Message).ConfigureAwait(false);
        }
    }

    private static Task WriteValidationProblemAsync(
        HttpContext context,
        ValidationException validationException)
    {
        var errors = validationException.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Uno o más campos no son válidos.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = context.Request.Path,
        };

        return WriteJsonAsync(context, StatusCodes.Status400BadRequest, problem);
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string? detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
        };

        return WriteJsonAsync(context, statusCode, problem);
    }

    private static Task WriteJsonAsync(HttpContext context, int statusCode, object payload)
    {
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = MediaTypeNames.Application.Json;
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, SerializerOptions));
    }
}

internal static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
