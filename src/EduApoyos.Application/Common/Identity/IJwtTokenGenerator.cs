namespace EduApoyos.Application.Common.Identity;

/// <summary>
/// Emits signed JWT access tokens for authenticated users. The concrete
/// implementation lives in <c>EduApoyos.Infrastructure</c> so the Application
/// layer stays independent from any specific JWT library.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Builds a signed JWT for <paramref name="user"/>. The returned
    /// <see cref="AccessToken"/> is intended to be shipped as-is to the caller.
    /// </summary>
    AccessToken Generate(UserSummary user);
}

/// <summary>
/// Encapsulates the JWT string together with its absolute UTC expiration so
/// the API can surface both values to the client without recomputing them.
/// </summary>
public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);
