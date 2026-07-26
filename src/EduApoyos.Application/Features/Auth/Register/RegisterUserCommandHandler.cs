using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.Auth.Register;

/// <summary>
/// Orchestrates the creation of a new user via <see cref="IIdentityService"/>.
/// Business rules enforced here (see US-004):
/// <list type="bullet">
///   <item>RN-002 / RN-003: password hashing and user creation are delegated to Identity.</item>
///   <item>RN-004: <c>RegisteredAt</c> is stamped by the identity layer.</item>
///   <item>RN-006: no JWT is issued.</item>
/// </list>
/// </summary>
public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IIdentityService _identityService;

    public RegisterUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService
            .CreateUserAsync(
                request.FullName.Trim(),
                request.Email.Trim(),
                request.Password,
                request.Role,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(result.Error);
        }

        var user = result.Value;
        var response = new RegisterUserResponse(
            user.Id,
            user.Email,
            user.FullName,
            (int)user.Role,
            user.RegisteredAt);

        return Result.Success(response);
    }
}
