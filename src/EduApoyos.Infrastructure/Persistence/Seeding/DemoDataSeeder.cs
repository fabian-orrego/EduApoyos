using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using EduApoyos.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduApoyos.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotent demo dataset applied after migrations so a fresh environment already has an
/// Advisor, 20 Students and a varied catalog of support requests/history for manual testing.
/// </summary>
public static class DemoDataSeeder
{
    private const string AdvisorEmail = "asesor@eduapoyos.local";
    private const string AdvisorPassword = "Advisor1234*";
    private const string AdvisorFullName = "Carolina Mejía Ríos";

    private static readonly string[] Programs =
    [
        "Ingeniería de Software",
        "Ingeniería Industrial",
        "Administración de Empresas",
        "Contaduría Pública",
    ];

    private static readonly string[] FirstNames =
    [
        "Andrés", "Valentina", "Juan", "Camila", "Santiago",
        "Mariana", "Daniel", "Laura", "Sebastián", "Natalia",
        "Felipe", "Diana", "Mateo", "Isabella", "Tomás",
        "Sofía", "Nicolás", "Juliana", "David", "Paula",
    ];

    private static readonly string[] LastNames =
    [
        "García López", "Rodríguez Pérez", "Martínez Gómez", "Hernández Díaz",
        "López Vargas", "González Ruiz", "Pérez Castro", "Sánchez Mora",
        "Ramírez Soto", "Torres Niño", "Flores Quintero", "Rivera Cárdenas",
        "Jiménez Peña", "Morales Duarte", "Ortiz Beltrán", "Gutiérrez Ríos",
        "Castillo Vega", "Romero Patiño", "Vargas Méndez", "Silva Acosta",
    ];

    /// <summary>
    /// Requests-per-student layout: students 1-5 → 1, 6-10 → 2, 11-15 → 3, 16-19 → 4, 20 → 0.
    /// </summary>
    private static readonly int[] RequestsPerStudent =
    [
        1, 1, 1, 1, 1,
        2, 2, 2, 2, 2,
        3, 3, 3, 3, 3,
        4, 4, 4, 4,
        0,
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DemoDataSeeder));
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        if (await userManager.Users.AnyAsync(u => u.Email == AdvisorEmail, cancellationToken)
                .ConfigureAwait(false))
        {
            logger.LogInformation("Demo data already present; skipping seed.");
            return;
        }

        logger.LogInformation("Seeding demo Advisor, Students and SupportRequests...");

        var advisor = await CreateUserAsync(
                userManager,
                AdvisorFullName,
                AdvisorEmail,
                AdvisorPassword,
                UserRole.Advisor,
                cancellationToken)
            .ConfigureAwait(false);

        var students = new List<(ApplicationUser User, Student Profile)>(capacity: 20);

        for (var index = 1; index <= 20; index++)
        {
            var email = $"estudiante{index}@eduapoyos.local";
            var password = $"Student{index}*";
            var fullName = $"{FirstNames[index - 1]} {LastNames[index - 1]}";

            var user = await CreateUserAsync(
                    userManager,
                    fullName,
                    email,
                    password,
                    UserRole.Student,
                    cancellationToken)
                .ConfigureAwait(false);

            var profile = new Student(
                userId: user.Id,
                documentNumber: $"{1000000000 + index}",
                documentType: (DocumentType)((index % 3) + 1),
                academicProgram: Programs[(index - 1) % Programs.Length],
                semester: ((index - 1) % 12) + 1);

            students.Add((user, profile));
            await db.Students.AddAsync(profile, cancellationToken).ConfigureAwait(false);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var requestIndex = 0;
        for (var i = 0; i < students.Count; i++)
        {
            var (user, profile) = students[i];
            var count = RequestsPerStudent[i];

            for (var n = 0; n < count; n++)
            {
                var targetStatus = PickStatus(requestIndex);
                var (request, history) = BuildSupportRequest(
                    studentId: profile.Id,
                    studentUserId: user.Id,
                    advisorId: advisor.Id,
                    supportType: (SupportType)((requestIndex % 3) + 1),
                    amount: 500_000m + (requestIndex * 150_000m),
                    description:
                        $"Solicitud de apoyo #{n + 1} de {user.FullName} para cubrir " +
                        $"matrícula y materiales del semestre {profile.Semester}.",
                    targetStatus: targetStatus,
                    reviewNotes: $"Asignada a revisión. Caso demo #{requestIndex + 1}.",
                    finalNotes: targetStatus switch
                    {
                        SupportRequestStatus.Approved =>
                            "Cumple los requisitos académicos y socioeconómicos. Aprobada.",
                        SupportRequestStatus.Rejected =>
                            "Documentación incompleta / no cumple puntaje mínimo. Rechazada.",
                        _ => null,
                    });

                await db.SupportRequests.AddAsync(request, cancellationToken).ConfigureAwait(false);
                await db.StatusHistories.AddRangeAsync(history, cancellationToken)
                    .ConfigureAwait(false);
                requestIndex++;
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation(
            "Demo seed completed: 1 Advisor, {StudentCount} Students, {RequestCount} SupportRequests.",
            students.Count,
            requestIndex);
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string fullName,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            Role = role,
            RegisteredAt = DateTime.UtcNow,
        };

        var createResult = await userManager.CreateAsync(user, password).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException(
                $"No se pudo crear el usuario demo '{email}': {errors}");
        }

        var roleResult = await userManager
            .AddToRoleAsync(user, ApplicationRoles.ToName(role))
            .ConfigureAwait(false);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException(
                $"No se pudo asignar el rol al usuario demo '{email}': {errors}");
        }

        return user;
    }

    private static SupportRequestStatus PickStatus(int requestIndex) =>
        (requestIndex % 4) switch
        {
            0 => SupportRequestStatus.Pending,
            1 => SupportRequestStatus.UnderReview,
            2 => SupportRequestStatus.Approved,
            _ => SupportRequestStatus.Rejected,
        };

    private static (SupportRequest Request, IReadOnlyList<StatusHistory> History) BuildSupportRequest(
        Guid studentId,
        Guid studentUserId,
        Guid advisorId,
        SupportType supportType,
        decimal amount,
        string description,
        SupportRequestStatus targetStatus,
        string? reviewNotes,
        string? finalNotes)
    {
        var request = new SupportRequest(studentId, supportType, amount, description);
        var history = new List<StatusHistory>
        {
            new(
                supportRequestId: request.Id,
                previousStatus: SupportRequestStatus.Pending,
                newStatus: SupportRequestStatus.Pending,
                changedByUserId: studentUserId,
                notes: "Solicitud creada."),
        };

        if (targetStatus == SupportRequestStatus.Pending)
        {
            return (request, history);
        }

        request.ChangeStatus(SupportRequestStatus.UnderReview, advisorId);
        history.Add(new StatusHistory(
            supportRequestId: request.Id,
            previousStatus: SupportRequestStatus.Pending,
            newStatus: SupportRequestStatus.UnderReview,
            changedByUserId: advisorId,
            notes: reviewNotes));

        if (targetStatus == SupportRequestStatus.UnderReview)
        {
            return (request, history);
        }

        request.ChangeStatus(targetStatus, advisorId);
        history.Add(new StatusHistory(
            supportRequestId: request.Id,
            previousStatus: SupportRequestStatus.UnderReview,
            newStatus: targetStatus,
            changedByUserId: advisorId,
            notes: finalNotes));

        return (request, history);
    }
}
