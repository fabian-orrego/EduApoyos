namespace EduApoyos.Application.Features.Students.Update;

/// <summary>
/// Public representation of a student after a successful update (US-009). <c>DocumentType</c>
/// mirrors the integer value of <see cref="Domain.Enums.DocumentType"/> so the API contract
/// stays language agnostic.
/// </summary>
public sealed record UpdateStudentResponse(
    Guid Id,
    Guid UserId,
    string DocumentNumber,
    int DocumentType,
    string AcademicProgram,
    int Semester);
