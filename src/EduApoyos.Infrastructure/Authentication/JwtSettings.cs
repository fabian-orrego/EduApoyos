namespace EduApoyos.Infrastructure.Authentication;

/// <summary>
/// Strongly-typed binding for the <c>Jwt</c> configuration section. The signing
/// key, issuer, audience and access-token lifetime are all sourced from
/// <c>appsettings.json</c> (RN-001, RN-002 of US-005). The API layer registers
/// this class through <c>IOptions</c> during startup.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Issuer claim (<c>iss</c>) embedded in every token.</summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>Audience claim (<c>aud</c>) embedded in every token.</summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>HMAC-SHA256 signing key. Must be at least 32 bytes long.</summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>Access-token lifetime (in minutes) reported to the client.</summary>
    public int AccessTokenExpirationMinutes { get; init; } = 60;
}
