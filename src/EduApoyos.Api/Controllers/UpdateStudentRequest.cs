using EduApoyos.Application.Features.Students.Update;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// HTTP payload for <c>PUT /api/estudiantes/{id}</c> (US-009). Kept separate from the
/// MediatR command so the transport model can evolve independently. <see cref="DocumentType"/>
/// is sent as the integer value of the <see cref="Domain.Enums.DocumentType"/> enum
/// (1 = NationalId, 2 = ForeignerId, 3 = Passport).
/// </summary>
public sealed record UpdateStudentRequest(
    string DocumentNumber,
    int DocumentType,
    string AcademicProgram,
    int Semester)
{
    internal UpdateStudentCommand ToCommand(Guid id) =>
        new(id, DocumentNumber, (DocumentType)DocumentType, AcademicProgram, Semester);
}
