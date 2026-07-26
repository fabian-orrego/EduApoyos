using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Common.Identity;

/// <summary>
/// Application abstraction over ASP.NET Core Identity. The concrete implementation lives in
/// <c>EduApoyos.Infrastructure</c> so this project stays independent from Identity/EF Core.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Creates a new user, hashes the password and attaches the requested role.
    /// </summary>
    /// <returns>
    /// <see cref="Result.Success{T}"/> with a <see cref="UserSummary"/> on success,
    /// <see cref="Result.Failure{T}"/> with a <see cref="ErrorType.Conflict"/> error when the email
    /// already exists, or a generic failure when Identity reports any other error.
    /// </returns>
    Task<Result<UserSummary>> CreateUserAsync(
        string fullName,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates the supplied credentials against ASP.NET Core Identity.
    /// </summary>
    /// <remarks>
    /// Per US-005 RN-004 the caller must not be able to tell whether the email or the password
    /// was wrong, therefore every failure path returns the same generic
    /// <see cref="ErrorType.Unauthorized"/> error.
    /// </remarks>
    Task<Result<UserSummary>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken);
}
