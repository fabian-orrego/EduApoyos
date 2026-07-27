using EduApoyos.Domain.Enums;
using FluentValidation;

namespace EduApoyos.Application.Features.SupportRequests.ChangeStatus;

/// <summary>
/// FluentValidation rules for <see cref="ChangeSupportRequestStatusCommand"/> (US-016). Note
/// that the transition rules themselves are encoded in the aggregate so this validator only
/// covers the "external" invariants: identifier presence, valid enum value and the
/// notes-required-when-Rejected constraint (RN-7).
/// </summary>
public sealed class ChangeSupportRequestStatusCommandValidator
    : AbstractValidator<ChangeSupportRequestStatusCommand>
{
    public const int NotesMaxLength = 500;

    public ChangeSupportRequestStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador de la solicitud es obligatorio.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("El estado seleccionado no es válido.");

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("La observación es obligatoria cuando la solicitud es rechazada.")
            .When(x => x.NewStatus == SupportRequestStatus.Rejected);

        RuleFor(x => x.Notes!)
            .MaximumLength(NotesMaxLength)
                .WithMessage($"La observación no puede superar {NotesMaxLength} caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
