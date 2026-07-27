using FluentValidation;

namespace EduApoyos.Application.Features.SupportRequests.Update;

/// <summary>
/// FluentValidation rules for <see cref="UpdateSupportRequestCommand"/> (US-016 nota #1).
/// Field validations mirror the ones enforced during creation so both flows behave the same.
/// </summary>
public sealed class UpdateSupportRequestCommandValidator
    : AbstractValidator<UpdateSupportRequestCommand>
{
    public const int DescriptionMaxLength = 1000;

    public UpdateSupportRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador de la solicitud es obligatorio.");

        RuleFor(x => x.SupportType)
            .IsInEnum().WithMessage("El tipo de apoyo no es válido.");

        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0m).WithMessage("El monto solicitado debe ser mayor que cero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(DescriptionMaxLength)
                .WithMessage(
                    $"La descripción no puede superar {DescriptionMaxLength} caracteres.");
    }
}
