namespace EduApoyos.Application.Features.Auth.Register;

/// <summary>
/// Public representation of a freshly registered user. The password hash is intentionally omitted.
/// <c>RoleId</c> mirrors the integer value of the <see cref="Domain.Enums.UserRole"/> enum
/// (1 = Advisor, 2 = Student), which is how the frontend identifies roles.
/// </summary>
public sealed record RegisterUserResponse(
    Guid Id,
    string Email,
    string FullName,
    int RoleId,
    DateTime RegisteredAt);
