namespace EduApoyos.Application.Common.Results;

/// <summary>
/// Domain-agnostic error descriptor used by the <see cref="Result"/> pattern.
/// The <see cref="Code"/> is a machine-friendly identifier, <see cref="Message"/> is
/// intended for user consumption (Spanish per project conventions).
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);
}
