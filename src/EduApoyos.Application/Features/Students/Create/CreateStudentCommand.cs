using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.Students.Create;

/// <summary>
/// Registers a new student in the platform (US-008). The command is dispatched by an Advisor and
/// links an existing Identity user (looked up by email) with the academic information.
/// </summary>
public sealed record CreateStudentCommand(
    string Email,
    string DocumentNumber,
    DocumentType DocumentType,
    string AcademicProgram,
    int Semester) : IRequest<Result<CreateStudentResponse>>;
