using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Common.Identity;

/// <summary>
/// Exposes the authenticated user information to the Application layer without leaking
/// <c>HttpContext</c>. The concrete implementation reads the claims from the current JWT and
/// lives in the API project so the handlers stay transport-agnostic.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Identifier of the authenticated user (matches the <c>sub</c>/<c>nameidentifier</c> JWT
    /// claim). Returns <c>null</c> when the caller is anonymous.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Role of the authenticated user, or <c>null</c> when the caller is anonymous.
    /// </summary>
    UserRole? Role { get; }

    /// <summary>
    /// Convenience flag: <c>true</c> when a valid user id is present.
    /// </summary>
    bool IsAuthenticated { get; }
}
