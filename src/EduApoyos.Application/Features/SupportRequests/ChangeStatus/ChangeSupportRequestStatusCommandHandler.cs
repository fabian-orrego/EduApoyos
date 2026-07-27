using EduApoyos.Application.Common.Identity;
using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.ChangeStatus;

/// <summary>
/// Orchestrates a status transition on a <see cref="SupportRequest"/> aggregate (US-016).
/// Business rules enforced:
/// <list type="bullet">
///   <item>RN-1: only the transitions defined in the aggregate are allowed.</item>
///   <item>RN-2 / RN-3: finalized requests cannot change state.</item>
///   <item>RN-4: <c>UpdatedAt</c> is refreshed by the aggregate.</item>
///   <item>RN-5: the advisor that performed the change is recorded (current user).</item>
///   <item>RN-6: a new <see cref="StatusHistory"/> entry is created atomically.</item>
///   <item>RN-7: the notes field is required when moving to Rejected (validator).</item>
/// </list>
/// </summary>
public sealed class ChangeSupportRequestStatusCommandHandler
    : IRequestHandler<ChangeSupportRequestStatusCommand, Result<ChangeSupportRequestStatusResponse>>
{
    private readonly ISupportRequestRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public ChangeSupportRequestStatusCommandHandler(
        ISupportRequestRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<ChangeSupportRequestStatusResponse>> Handle(
        ChangeSupportRequestStatusCommand request,
        CancellationToken cancellationToken)
    {
        var advisorId = _currentUser.UserId;
        if (advisorId is null || _currentUser.Role != UserRole.Advisor)
        {
            return Result.Failure<ChangeSupportRequestStatusResponse>(
                Error.Forbidden(
                    "supportRequests.status.forbidden",
                    "Solo los asesores pueden cambiar el estado de una solicitud."));
        }

        var supportRequest = await _repository
            .GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (supportRequest is null)
        {
            return Result.Failure<ChangeSupportRequestStatusResponse>(
                Error.NotFound(
                    "supportRequests.notFound",
                    "La solicitud no existe."));
        }

        if (supportRequest.IsFinalized)
        {
            return Result.Failure<ChangeSupportRequestStatusResponse>(
                Error.Conflict(
                    "supportRequests.status.finalized",
                    "La solicitud ya fue aprobada o rechazada y no puede modificarse."));
        }

        if (!SupportRequest.IsTransitionAllowed(supportRequest.Status, request.NewStatus))
        {
            return Result.Failure<ChangeSupportRequestStatusResponse>(
                Error.Conflict(
                    "supportRequests.status.invalidTransition",
                    $"No es posible cambiar de {supportRequest.Status} a {request.NewStatus}."));
        }

        var previousStatus = supportRequest.Status;
        supportRequest.ChangeStatus(request.NewStatus, advisorId.Value);

        var history = new StatusHistory(
            supportRequestId: supportRequest.Id,
            previousStatus: previousStatus,
            newStatus: request.NewStatus,
            changedByUserId: advisorId.Value,
            notes: string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim());

        await _repository
            .UpdateAsync(supportRequest, history, cancellationToken)
            .ConfigureAwait(false);

        var response = new ChangeSupportRequestStatusResponse(
            supportRequest.Id,
            (int)previousStatus,
            (int)supportRequest.Status,
            advisorId.Value,
            supportRequest.UpdatedAt);

        return Result.Success(response);
    }
}
