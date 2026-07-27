namespace EduApoyos.Application.Features.Auth.Login;

/// <summary>
/// Public representation of a successful login (US-005). Contains the signed JWT together with
/// the display information required by the frontend to greet the user and redirect based on the
/// role. <c>RoleId</c> matches the integer value of <see cref="Domain.Enums.UserRole"/>
/// (1 = Advisor, 2 = Student). <c>Email</c> is exposed so the SPA can pre-fill student-owned
/// forms without decoding the JWT.
/// </summary>
public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    string FullName,
    string Email,
    int RoleId);
