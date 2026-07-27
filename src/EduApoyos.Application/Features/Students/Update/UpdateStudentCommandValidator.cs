using FluentValidation;

namespace EduApoyos.Application.Features.Students.Update;

/// <summary>
/// FluentValidation rules for <see cref="UpdateStudentCommand"/> (US-009). Field validations
/// mirror the ones enforced during creation so both flows behave consistently.
/// </summary>
public sealed class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    private const int DocumentNumberMaxLength = 20;
    private const int AcademicProgramMaxLength = 150;
    private const int MinSemester = 1;
    private const int MaxSemester = 12;

    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador del estudiante es obligatorio.");

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
