using EduApoyos.Application.Common.Persistence;
using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.SupportRequests.Update;

/// <summary>
/// Orchestrates the update of the editable business fields of a support request (US-016
/// nota #1). Business rules enforced:
/// <list type="bullet">
///   <item>The request must exist.</item>
///   <item>Finalized requests (approved / rejected) are immutable (RN-2 / RN-3).</item>
///   <item>The <c>UpdatedAt</c> timestamp is refreshed by the aggregate itself.</item>
/// </list>
/// Field validations are enforced by <see cref="UpdateSupportRequestCommandValidator"/>.
/// </summary>
public sealed class UpdateSupportRequestCommandHandler
    : IRequestHandler<UpdateSupportRequestCommand, Result<UpdateSupportRequestResponse>>
{
    private readonly ISupportRequestRepository _repository;

    public UpdateSupportRequestCommandHandler(ISupportRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UpdateSupportRequestResponse>> Handle(
        UpdateSupportRequestCommand request,
        CancellationToken cancellationToken)
    {
        var supportRequest = await _repository
            .GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (supportRequest is null)
        {
            return Result.Failure<UpdateSupportRequestResponse>(
                Error.NotFound(
                    "supportRequests.notFound",
                    "La solicitud no existe."));
        }

        if (supportRequest.IsFinalized)
        {
            return Result.Failure<UpdateSupportRequestResponse>(
                Error.Conflict(
                    "supportRequests.update.finalized",
                    "La solicitud no puede modificarse porque ya fue aprobada o rechazada."));
        }

        supportRequest.UpdateDetails(
            request.SupportType,
            request.RequestedAmount,
            request.Description.Trim());

        await _repository
            .UpdateAsync(supportRequest, history: null, cancellationToken)
            .ConfigureAwait(false);

        var response = new UpdateSupportRequestResponse(
            supportRequest.Id,
            supportRequest.StudentId,
            (int)supportRequest.SupportType,
            supportRequest.RequestedAmount,
            supportRequest.Description,
            (int)supportRequest.Status,
            supportRequest.RequestedAt,
            supportRequest.UpdatedAt);

        return Result.Success(response);
    }
}
