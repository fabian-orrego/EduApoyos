using FluentValidation;

namespace EduApoyos.Application.Features.SupportRequests.Create;

/// <summary>
/// FluentValidation rules for <see cref="CreateSupportRequestCommand"/> (US-013). Messages are
/// user-facing (Spanish) per project conventions.
/// </summary>
public sealed class CreateSupportRequestCommandValidator
    : AbstractValidator<CreateSupportRequestCommand>
{
    public const int DescriptionMaxLength = 1000;

    public CreateSupportRequestCommandValidator()
    {
        RuleFor(x => x.StudentEmail)
            .NotEmpty().WithMessage("El correo del estudiante es obligatorio.")
            .EmailAddress().WithMessage("El correo del estudiante no tiene un formato válido.");

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
