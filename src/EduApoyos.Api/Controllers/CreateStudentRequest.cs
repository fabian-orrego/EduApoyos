using EduApoyos.Application.Features.Students.Create;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// HTTP payload for <c>POST /api/estudiantes</c> (US-008). Kept separate from the MediatR command
/// so the transport model can evolve independently. <see cref="DocumentType"/> is sent as the
/// integer value of the <see cref="Domain.Enums.DocumentType"/> enum (1 = NationalId,
/// 2 = ForeignerId, 3 = Passport).
/// </summary>
public sealed record CreateStudentRequest(
    string Email,
    string DocumentNumber,
    int DocumentType,
    string AcademicProgram,
    int Semester)
{
    internal CreateStudentCommand ToCommand() =>
        new(Email, DocumentNumber, (DocumentType)DocumentType, AcademicProgram, Semester);
}
