using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduApoyos.Application.Common.Identity;
using EduApoyos.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduApoyos.Infrastructure.Authentication;

/// <summary>
/// Default <see cref="IJwtTokenGenerator"/> implementation. Signs tokens with the
/// HMAC-SHA256 key configured in <see cref="JwtSettings"/> and embeds the claims
/// required by US-005: <c>sub</c> (user id), <c>email</c> and <c>role</c>.
/// </summary>
internal sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtTokenGenerator(IOptions<JwtSettings> options)
    {
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured. Add 'Jwt:SecretKey' to appsettings.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(_settings.SecretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    public AccessToken Generate(UserSummary user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, ApplicationRoles.ToName(user.Role)),
            new("roleId", ((int)user.Role).ToString(CultureInfo.InvariantCulture)),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: _signingCredentials);

        var serialized = _tokenHandler.WriteToken(token);
        return new AccessToken(serialized, expires);
    }
}
