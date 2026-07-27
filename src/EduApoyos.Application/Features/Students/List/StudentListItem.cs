namespace EduApoyos.Application.Features.Students.List;

/// <summary>
/// Read-model projection returned by <see cref="GetStudentsQuery"/> (US-011). The advisor
/// grid needs the student's identity information plus the linked user's full name and
/// email, so those two fields are joined from Identity's <c>AspNetUsers</c> table.
/// <c>DocumentType</c> is exposed as an integer so the API contract stays language agnostic
/// (see <see cref="Domain.Enums.DocumentType"/>).
/// </summary>
public sealed record StudentListItem(
    Guid Id,
    string FullName,
    string DocumentNumber,
    int DocumentType,
    string AcademicProgram,
    int Semester,
    string Email);
