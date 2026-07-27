using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.Students.Update;

/// <summary>
/// Updates the academic information of an existing student (US-009). The associated
/// <c>UserId</c> is deliberately excluded from the command because RN-003 forbids
/// reassigning the underlying user.
/// </summary>
public sealed record UpdateStudentCommand(
    Guid Id,
    string DocumentNumber,
    DocumentType DocumentType,
    string AcademicProgram,
    int Semester) : IRequest<Result<UpdateStudentResponse>>;
