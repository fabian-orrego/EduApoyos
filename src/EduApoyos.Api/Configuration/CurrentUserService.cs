using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EduApoyos.Application.Common.Identity;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Configuration;

/// <summary>
/// <see cref="ICurrentUserService"/> implementation backed by <see cref="IHttpContextAccessor"/>.
/// Reads the identifier from the standard JWT claims (<c>sub</c> or
/// <see cref="ClaimTypes.NameIdentifier"/>) and the role from the custom <c>roleId</c> claim
/// emitted by <c>JwtTokenGenerator</c> so the Application layer can perform ownership checks
/// without depending on transport concerns.
/// </summary>
internal sealed class CurrentUserService : ICurrentUserService
{
    private const string RoleIdClaim = "roleId";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var raw = user.FindFirstValue(RoleIdClaim);
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                && Enum.IsDefined(typeof(UserRole), value))
            {
                return (UserRole)value;
            }

            return null;
        }
    }

    public bool IsAuthenticated => UserId is not null;
}
