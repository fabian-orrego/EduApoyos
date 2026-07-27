using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.Students.Delete;

/// <summary>
/// Orchestrates the deletion of a <see cref="Domain.Entities.Student"/> (US-010). Business
/// rules enforced:
/// <list type="bullet">
///   <item>RN-1: a student with associated support requests cannot be deleted.</item>
///   <item>RN-2: a missing student is surfaced as <see cref="ErrorType.NotFound"/>.</item>
/// </list>
/// </summary>
public sealed class DeleteStudentCommandHandler
    : IRequestHandler<DeleteStudentCommand, Result>
{
    private readonly IStudentRepository _studentRepository;

    public DeleteStudentCommandHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result> Handle(
        DeleteStudentCommand request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository
            .GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (student is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "students.notFound",
                    "El estudiante no existe."));
        }

        var hasSupportRequests = await _studentRepository
            .HasSupportRequestsAsync(student.Id, cancellationToken)
            .ConfigureAwait(false);

        if (hasSupportRequests)
        {
            return Result.Failure(
                Error.Conflict(
                    "students.delete.hasSupportRequests",
                    "El estudiante tiene solicitudes asociadas y no puede eliminarse."));
        }

        await _studentRepository.DeleteAsync(student, cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
