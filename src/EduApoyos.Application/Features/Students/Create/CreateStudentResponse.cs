namespace EduApoyos.Application.Features.Students.Create;

/// <summary>
/// Public representation of a freshly created student (US-008). <c>DocumentType</c> mirrors the
/// integer value of <see cref="Domain.Enums.DocumentType"/> so the API contract stays language
/// agnostic.
/// </summary>
public sealed record CreateStudentResponse(
    Guid Id,
    Guid UserId,
    string DocumentNumber,
    int DocumentType,
    string AcademicProgram,
    int Semester);
