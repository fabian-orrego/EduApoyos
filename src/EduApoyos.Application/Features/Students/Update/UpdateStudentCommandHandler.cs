using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Entities;
using MediatR;

namespace EduApoyos.Application.Features.Students.Update;

/// <summary>
/// Orchestrates the update of an existing <see cref="Student"/> (US-009). Business rules
/// enforced:
/// <list type="bullet">
///   <item>The student must exist (otherwise a <see cref="ErrorType.NotFound"/> error is returned).</item>
///   <item>The document number must remain unique across the rest of the population.</item>
///   <item>The associated <c>UserId</c> is never modified (RN-003).</item>
/// </list>
/// Field-level validations are enforced by <see cref="UpdateStudentCommandValidator"/>.
/// </summary>
public sealed class UpdateStudentCommandHandler
    : IRequestHandler<UpdateStudentCommand, Result<UpdateStudentResponse>>
{
    private readonly IStudentRepository _studentRepository;

    public UpdateStudentCommandHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<UpdateStudentResponse>> Handle(
        UpdateStudentCommand request,
        CancellationToken cancellationToken)
    {
        var documentNumber = request.DocumentNumber.Trim();
        var academicProgram = request.AcademicProgram.Trim();

        var student = await _studentRepository
            .GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (student is null)
        {
            return Result.Failure<UpdateStudentResponse>(
                Error.NotFound(
                    "students.notFound",
                    "El estudiante no existe."));
        }

        var documentChanged =
            student.DocumentType != request.DocumentType
            || !string.Equals(student.DocumentNumber, documentNumber, StringComparison.Ordinal);

        if (documentChanged)
        {
            var duplicated = await _studentRepository
                .ExistsByDocumentAsync(
                    request.DocumentType,
                    documentNumber,
                    student.Id,
                    cancellationToken)
                .ConfigureAwait(false);

            if (duplicated)
            {
                return Result.Failure<UpdateStudentResponse>(
                    Error.Conflict(
                        "students.document.duplicated",
                        "El número de documento ya se encuentra registrado."));
            }
        }

        student.UpdateAcademicInfo(
            documentNumber,
            request.DocumentType,
            academicProgram,
            request.Semester);

        await _studentRepository.UpdateAsync(student, cancellationToken).ConfigureAwait(false);

        var response = new UpdateStudentResponse(
            student.Id,
            student.UserId,
            student.DocumentNumber,
            (int)student.DocumentType,
            student.AcademicProgram,
            student.Semester);

        return Result.Success(response);
    }
}
