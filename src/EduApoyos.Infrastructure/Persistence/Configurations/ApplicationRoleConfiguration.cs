using EduApoyos.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduApoyos.Infrastructure.Persistence.Configurations;

/// <summary>
/// Seeds the two roles required by US-004 (Advisor / Student). Concurrency stamps are
/// hard-coded so the migration snapshot stays deterministic across builds.
/// </summary>
internal sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.HasData(
            new ApplicationRole
            {
                Id = ApplicationRoles.AdvisorRoleId,
                Name = ApplicationRoles.Advisor,
                NormalizedName = ApplicationRoles.Advisor.ToUpperInvariant(),
                ConcurrencyStamp = "00000000-0000-0000-0000-000000000001",
            },
            new ApplicationRole
            {
                Id = ApplicationRoles.StudentRoleId,
                Name = ApplicationRoles.Student,
                NormalizedName = ApplicationRoles.Student.ToUpperInvariant(),
                ConcurrencyStamp = "00000000-0000-0000-0000-000000000002",
            });
    }
}
