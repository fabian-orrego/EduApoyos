using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EduApoyos.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity backed implementation of <see cref="IIdentityService"/>. Handles user
/// creation, password hashing (RN-002 / RN-003), stamps the requested role directly on the
/// <see cref="ApplicationUser.Role"/> column and links the user to the corresponding entry in
/// <c>AspNetRoles</c> so <see cref="RoleManager{TRole}"/> and Identity-based authorization work
/// out of the box (RN-005).
/// </summary>
internal sealed class IdentityService : IIdentityService
{
    // Shared generic error so we cannot leak whether the email or the password was invalid.
    private static readonly Error InvalidCredentialsError = Error.Unauthorized(
        "auth.credentials.invalid",
        "Credenciales inválidas.");

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<UserSummary>> CreateUserAsync(
        string fullName,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existing = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (existing is not null)
        {
            return Result.Failure<UserSummary>(
                Error.Conflict(
                    "auth.email.duplicated",
                    "El correo electrónico ya se encuentra registrado."));
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            Role = role,
            RegisteredAt = DateTime.UtcNow,
        };

        var createResult = await _userManager.CreateAsync(user, password).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            _logger.LogWarning(
                "Identity refused to create user {Email}: {Errors}",
                email,
                string.Join(", ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}")));
            return MapIdentityFailure(createResult);
        }

        var roleName = ApplicationRoles.ToName(role);
        var addRoleResult = await _userManager.AddToRoleAsync(user, roleName).ConfigureAwait(false);
        if (!addRoleResult.Succeeded)
        {
            _logger.LogError(
                "Failed to attach role {Role} to user {UserId}: {Errors}",
                roleName,
                user.Id,
                string.Join(", ", addRoleResult.Errors.Select(e => $"{e.Code}:{e.Description}")));

            // Roll back the user so the operation stays atomic from the caller's point of view.
            await _userManager.DeleteAsync(user).ConfigureAwait(false);
            return MapIdentityFailure(addRoleResult);
        }

        var summary = new UserSummary(
            user.Id,
            user.Email!,
            user.FullName,
            user.Role,
            user.RegisteredAt);

        return Result.Success(summary);
    }

    public async Task<UserSummary?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user is null)
        {
            return null;
        }

        return new UserSummary(
            user.Id,
            user.Email!,
            user.FullName,
            user.Role,
            user.RegisteredAt);
    }

    public async Task<Result<UserSummary>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user is null)
        {
            // RN-004: keep the failure reason opaque; only log the outcome for diagnostics.
            _logger.LogInformation("Login attempt for unknown email {Email}.", email);
            return Result.Failure<UserSummary>(InvalidCredentialsError);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password).ConfigureAwait(false);
        if (!passwordValid)
        {
            _logger.LogInformation("Login attempt with wrong password for user {UserId}.", user.Id);
            return Result.Failure<UserSummary>(InvalidCredentialsError);
        }

        var summary = new UserSummary(
            user.Id,
            user.Email!,
            user.FullName,
            user.Role,
            user.RegisteredAt);

        return Result.Success(summary);
    }

    private static Result<UserSummary> MapIdentityFailure(IdentityResult identityResult)
    {
        var duplicated = identityResult.Errors.FirstOrDefault(e =>
            string.Equals(e.Code, "DuplicateEmail", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.Code, "DuplicateUserName", StringComparison.OrdinalIgnoreCase));

        if (duplicated is not null)
        {
            return Result.Failure<UserSummary>(
                Error.Conflict(
                    "auth.email.duplicated",
                    "El correo electrónico ya se encuentra registrado."));
        }

        var description = identityResult.Errors.FirstOrDefault()?.Description
            ?? "No fue posible completar el registro del usuario.";

        return Result.Failure<UserSummary>(
            Error.Validation("auth.identity.failure", description));
    }
}
