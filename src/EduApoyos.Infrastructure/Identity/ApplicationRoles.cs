using EduApoyos.Domain.Enums;

namespace EduApoyos.Infrastructure.Identity;

/// <summary>
/// Deterministic definition of the roles supported by the platform. ASP.NET Core Identity forces
/// role IDs to share the user's key type (<see cref="Guid"/>), so the Guids are kept intentionally
/// minimal (only the last digit varies) to mirror the incremental <see cref="UserRole"/> values.
/// The API contract still exposes roles as a plain integer <c>RoleId</c>.
/// </summary>
internal static class ApplicationRoles
{
    public const string Advisor = "Advisor";
    public const string Student = "Student";

    public static readonly Guid AdvisorRoleId =
        new("00000000-0000-0000-0000-000000000001");

    public static readonly Guid StudentRoleId =
        new("00000000-0000-0000-0000-000000000002");

    public static string ToName(UserRole role) => role switch
    {
        UserRole.Advisor => Advisor,
        UserRole.Student => Student,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Rol de usuario no soportado."),
    };
}
