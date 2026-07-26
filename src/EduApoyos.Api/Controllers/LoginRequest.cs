using EduApoyos.Application.Features.Auth.Login;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// HTTP payload for <c>POST /api/auth/login</c>. Kept separate from the MediatR command so the
/// transport model can evolve independently from the application contract.
/// </summary>
public sealed record LoginRequest(string Email, string Password)
{
    internal LoginCommand ToCommand() => new(Email, Password);
}
