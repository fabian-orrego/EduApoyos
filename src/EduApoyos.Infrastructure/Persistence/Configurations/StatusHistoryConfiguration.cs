using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduApoyos.Infrastructure.Persistence.Configurations;

internal sealed class StatusHistoryConfiguration : IEntityTypeConfiguration<StatusHistory>
{
    public void Configure(EntityTypeBuilder<StatusHistory> builder)
    {
        builder.ToTable("StatusHistories");

        builder.HasKey(sh => sh.Id);

        builder.Property(sh => sh.Id)
            .ValueGeneratedNever();

        builder.Property(sh => sh.SupportRequestId)
            .IsRequired();

        builder.Property(sh => sh.PreviousStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sh => sh.NewStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sh => sh.ChangedAt)
            .IsRequired();

        builder.Property(sh => sh.ChangedByUserId)
            .IsRequired();

        builder.Property(sh => sh.Notes)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.HasIndex(sh => sh.SupportRequestId)
            .HasDatabaseName("IX_StatusHistories_SupportRequestId");

        builder.HasIndex(sh => sh.ChangedByUserId)
            .HasDatabaseName("IX_StatusHistories_ChangedByUserId");

        builder.HasIndex(sh => new { sh.SupportRequestId, sh.ChangedAt })
            .HasDatabaseName("IX_StatusHistories_SupportRequestId_ChangedAt");

        builder.HasOne<SupportRequest>()
            .WithMany()
            .HasForeignKey(sh => sh.SupportRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(sh => sh.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
