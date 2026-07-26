namespace EduApoyos.Application.Common.Results;

/// <summary>
/// Categorizes an <see cref="Error"/> so the API layer can translate it into the
/// appropriate HTTP status code without leaking transport concerns into the Application layer.
/// </summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
}
