using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.Create;

/// <summary>
/// Orchestrates the creation of a new <see cref="SupportRequest"/> (US-013). Business rules
/// enforced:
/// <list type="bullet">
///   <item>RN-1: the student identified by email must exist.</item>
///   <item>RN-2: the initial status is always <see cref="SupportRequestStatus.Pending"/>.</item>
///   <item>RN-3/RN-4: request/update timestamps are set by the aggregate.</item>
///   <item>RN-4: a first history entry is created together with the request.</item>
///   <item>RN-5: the advisor is optional at creation and is therefore never assigned here.</item>
/// </list>
/// The caller (advisor or student) is recorded in the initial history entry so the timeline is
/// complete from the very first event.
/// </summary>
public sealed class CreateSupportRequestCommandHandler
    : IRequestHandler<CreateSupportRequestCommand, Result<CreateSupportRequestResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IStudentRepository _studentRepository;
    private readonly ISupportRequestRepository _supportRequestRepository;
    private readonly ICurrentUserService _currentUser;

    public CreateSupportRequestCommandHandler(
        IIdentityService identityService,
        IStudentRepository studentRepository,
        ISupportRequestRepository supportRequestRepository,
        ICurrentUserService currentUser)
    {
        _identityService = identityService;
        _studentRepository = studentRepository;
        _supportRequestRepository = supportRequestRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<CreateSupportRequestResponse>> Handle(
        CreateSupportRequestCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.StudentEmail.Trim();
        var description = request.Description.Trim();

        var user = await _identityService
            .FindByEmailAsync(email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || user.Role != UserRole.Student)
        {
            return Result.Failure<CreateSupportRequestResponse>(
                Error.NotFound(
                    "supportRequests.student.notFound",
                    "El estudiante no existe."));
        }

        var studentId = await _studentRepository
            .GetIdByUserIdAsync(user.Id, cancellationToken)
            .ConfigureAwait(false);

        if (studentId is null)
        {
            return Result.Failure<CreateSupportRequestResponse>(
                Error.NotFound(
                    "supportRequests.student.notFound",
                    "El estudiante no existe."));
        }

        // The initial history entry must be traceable to a real user. When the request is
        // created by the advisor itself we log the advisor, otherwise we default to the student
        // user id so the record satisfies the FK to AspNetUsers.
        var actorUserId = _currentUser.UserId ?? user.Id;

        var supportRequest = new SupportRequest(
            studentId.Value,
            request.SupportType,
            request.RequestedAmount,
            description);

        var initialHistory = new StatusHistory(
            supportRequest.Id,
            previousStatus: SupportRequestStatus.Pending,
            newStatus: SupportRequestStatus.Pending,
            changedByUserId: actorUserId,
            notes: "Solicitud creada.");

        await _supportRequestRepository
            .CreateAsync(supportRequest, initialHistory, cancellationToken)
            .ConfigureAwait(false);

        var response = new CreateSupportRequestResponse(
            supportRequest.Id,
            supportRequest.StudentId,
            (int)supportRequest.SupportType,
            supportRequest.RequestedAmount,
            supportRequest.Description,
            (int)supportRequest.Status,
            supportRequest.RequestedAt);

        return Result.Success(response);
    }
}
