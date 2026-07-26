using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.Students.Create;

/// <summary>
/// Orchestrates the creation of a new <see cref="Student"/> (US-008). Business rules enforced:
/// <list type="bullet">
///   <item>RN-001: the user must exist (looked up by email).</item>
///   <item>RN-002: the user must have the <see cref="UserRole.Student"/> role.</item>
///   <item>RN-003: the user cannot be associated to another student.</item>
///   <item>RN-004: the document number must be unique.</item>
/// </list>
/// The semester range (RN-005) is enforced by <see cref="CreateStudentCommandValidator"/>.
/// </summary>
public sealed class CreateStudentCommandHandler
    : IRequestHandler<CreateStudentCommand, Result<CreateStudentResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IStudentRepository _studentRepository;

    public CreateStudentCommandHandler(
        IIdentityService identityService,
        IStudentRepository studentRepository)
    {
        _identityService = identityService;
        _studentRepository = studentRepository;
    }

    public async Task<Result<CreateStudentResponse>> Handle(
        CreateStudentCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var documentNumber = request.DocumentNumber.Trim();
        var academicProgram = request.AcademicProgram.Trim();

        var user = await _identityService
            .FindByEmailAsync(email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure<CreateStudentResponse>(
                Error.Validation(
                    "students.user.notFound",
                    "El usuario asociado no existe."));
        }

        if (user.Role != UserRole.Student)
        {
            return Result.Failure<CreateStudentResponse>(
                Error.Validation(
                    "students.user.invalidRole",
                    "El usuario debe tener rol Estudiante."));
        }

        var userAlreadyLinked = await _studentRepository
            .ExistsByUserIdAsync(user.Id, cancellationToken)
            .ConfigureAwait(false);
        if (userAlreadyLinked)
        {
            return Result.Failure<CreateStudentResponse>(
                Error.Conflict(
                    "students.user.alreadyLinked",
                    "El usuario ya está asociado a otro estudiante."));
        }

        var documentAlreadyRegistered = await _studentRepository
            .ExistsByDocumentAsync(request.DocumentType, documentNumber, cancellationToken)
            .ConfigureAwait(false);
        if (documentAlreadyRegistered)
        {
            return Result.Failure<CreateStudentResponse>(
                Error.Conflict(
                    "students.document.duplicated",
                    "El número de documento ya se encuentra registrado."));
        }

        var student = new Student(
            user.Id,
            documentNumber,
            request.DocumentType,
            academicProgram,
            request.Semester);

        await _studentRepository.CreateAsync(student, cancellationToken).ConfigureAwait(false);

        var response = new CreateStudentResponse(
            student.Id,
            student.UserId,
            student.DocumentNumber,
            (int)student.DocumentType,
            student.AcademicProgram,
            student.Semester);

        return Result.Success(response);
    }
}
