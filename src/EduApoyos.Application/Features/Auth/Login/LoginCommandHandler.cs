using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.Auth.Login;

/// <summary>
/// Orchestrates the login flow (US-005):
/// <list type="number">
///   <item>Delegates credential validation to <see cref="IIdentityService"/>.</item>
///   <item>On success, asks <see cref="IJwtTokenGenerator"/> for a signed JWT.</item>
///   <item>Returns a <see cref="LoginResponse"/> with the token, expiration, full name and role.</item>
/// </list>
/// Any credential failure is propagated as-is so it can be surfaced as HTTP 401 with a generic
/// message per RN-004.
/// </summary>
public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtTokenGenerator tokenGenerator)
    {
        _identityService = identityService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var credentialResult = await _identityService
            .ValidateCredentialsAsync(
                request.Email.Trim(),
                request.Password,
                cancellationToken)
            .ConfigureAwait(false);

        if (credentialResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(credentialResult.Error);
        }

        var user = credentialResult.Value;
        var accessToken = _tokenGenerator.Generate(user);

        var response = new LoginResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            user.FullName,
            (int)user.Role);

        return Result.Success(response);
    }
}
