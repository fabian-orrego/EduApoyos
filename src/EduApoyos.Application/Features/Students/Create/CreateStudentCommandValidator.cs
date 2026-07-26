using EduApoyos.Domain.Enums;
using FluentValidation;

namespace EduApoyos.Application.Features.Students.Create;

/// <summary>
/// FluentValidation rules for <see cref="CreateStudentCommand"/> (US-008). Messages are
/// user-facing (Spanish) per project conventions.
/// </summary>
public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    private const int DocumentNumberMaxLength = 20;
    private const int AcademicProgramMaxLength = 150;
    private const int MinSemester = 1;
    private const int MaxSemester = 12;

    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .MaximumLength(DocumentNumberMaxLength)
                .WithMessage(
                    $"El número de documento no puede superar {DocumentNumberMaxLength} caracteres.");

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("El tipo de documento no es válido.");

        RuleFor(x => x.AcademicProgram)
            .NotEmpty().WithMessage("El programa académico es obligatorio.")
            .MaximumLength(AcademicProgramMaxLength)
                .WithMessage(
                    $"El programa académico no puede superar {AcademicProgramMaxLength} caracteres.");

        RuleFor(x => x.Semester)
            .InclusiveBetween(MinSemester, MaxSemester)
                .WithMessage($"El semestre debe estar entre {MinSemester} y {MaxSemester}.");
    }
}
