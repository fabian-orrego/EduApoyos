using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduApoyos.Infrastructure.Persistence.Configurations;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.DocumentNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(s => s.DocumentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.AcademicProgram)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Semester)
            .IsRequired();

        builder.HasIndex(s => s.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Students_UserId");

        builder.HasIndex(s => new { s.DocumentType, s.DocumentNumber })
            .IsUnique()
            .HasDatabaseName("IX_Students_DocumentType_DocumentNumber");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Students_Semester_Range",
            "[Semester] >= 1 AND [Semester] <= 20"));

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
