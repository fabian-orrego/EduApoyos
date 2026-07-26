using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.Auth.Register;

/// <summary>
/// Registers a new user in the platform. Public endpoint: no JWT is issued (RN-006).
/// </summary>
public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    UserRole Role) : IRequest<Result<RegisterUserResponse>>;
