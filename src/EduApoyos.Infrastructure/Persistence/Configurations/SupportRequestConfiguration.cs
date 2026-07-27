using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduApoyos.Infrastructure.Persistence.Configurations;

internal sealed class SupportRequestConfiguration : IEntityTypeConfiguration<SupportRequest>
{
    public void Configure(EntityTypeBuilder<SupportRequest> builder)
    {
        builder.ToTable("SupportRequests");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Id)
            .ValueGeneratedNever();

        builder.Property(sr => sr.StudentId)
            .IsRequired();

        builder.Property(sr => sr.SupportType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sr => sr.RequestedAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(sr => sr.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(sr => sr.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sr => sr.RequestedAt)
            .IsRequired();

        builder.Property(sr => sr.UpdatedAt)
            .IsRequired();

        builder.Property(sr => sr.AdvisorId)
            .IsRequired(false);

        builder.HasIndex(sr => sr.StudentId)
            .HasDatabaseName("IX_SupportRequests_StudentId");

        // Composite index for aging Pending queries (Status + UpdatedAt range/order)
        // and for Status-only filters (leading key). Replaces the former Status-only index.
        builder.HasIndex(sr => new { sr.Status, sr.UpdatedAt })
            .HasDatabaseName("IX_SupportRequests_Status_UpdatedAt");

        builder.HasIndex(sr => sr.AdvisorId)
            .HasDatabaseName("IX_SupportRequests_AdvisorId");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SupportRequests_RequestedAmount_Positive",
            "[RequestedAmount] > 0"));

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(sr => sr.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(sr => sr.AdvisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
