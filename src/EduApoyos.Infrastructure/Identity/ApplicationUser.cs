using EduApoyos.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduApoyos.Infrastructure.Identity;

/// <summary>
/// Identity user aggregate for EduApoyos. It maps to the ASP.NET Core Identity <c>AspNetUsers</c>
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        FullName = string.Empty;
        RegisteredAt = DateTime.UtcNow;
    }

    public string FullName { get; set; }

    public UserRole Role { get; set; }

    public DateTime RegisteredAt { get; set; }
}
