using System.Reflection;
using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Persistence;

/// <summary>
/// EF Core Code-First database context. It is the single source of truth for the schema
/// and combines ASP.NET Core Identity tables with the EduApoyos business tables.
/// </summary>
public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();

    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
