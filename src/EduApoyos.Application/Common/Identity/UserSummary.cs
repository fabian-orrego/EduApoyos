using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Common.Identity;

/// <summary>
/// Immutable projection of an ASP.NET Core Identity user exposed to the Application layer.
/// Keeps the Application project unaware of Identity types and EF Core entities.
/// </summary>
public sealed record UserSummary(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    DateTime RegisteredAt);
