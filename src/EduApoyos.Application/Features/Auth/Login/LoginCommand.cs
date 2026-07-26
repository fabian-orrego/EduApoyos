using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.Auth.Login;

/// <summary>
/// Authenticates a user against ASP.NET Core Identity and returns a signed JWT (US-005).
/// </summary>
public sealed record LoginCommand(string Email, string Password)
    : IRequest<Result<LoginResponse>>;
