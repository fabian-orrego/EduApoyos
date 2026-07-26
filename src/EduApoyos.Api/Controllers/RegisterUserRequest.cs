using EduApoyos.Application.Features.Auth.Register;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// HTTP payload for <c>POST /api/auth/register</c>. Kept separate from the MediatR command so
/// the transport model can evolve independently from the application contract. The client sends
/// <see cref="RoleId"/> as an integer that matches the <see cref="UserRole"/> enum values
/// (1 = Advisor, 2 = Student).
/// </summary>
public sealed record RegisterUserRequest(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    int RoleId)
{
    internal RegisterUserCommand ToCommand() =>
        new(FullName, Email, Password, ConfirmPassword, (UserRole)RoleId);
}
