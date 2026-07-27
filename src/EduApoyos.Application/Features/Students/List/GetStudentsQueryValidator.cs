using FluentValidation;

namespace EduApoyos.Application.Features.Students.List;

/// <summary>
/// FluentValidation rules for <see cref="GetStudentsQuery"/> (US-011). Enforces the mandatory
/// pagination parameters and the project-wide maximum page size of 100 records.
/// </summary>
public sealed class GetStudentsQueryValidator : AbstractValidator<GetStudentsQuery>
{
    public const int MaxPageSize = 100;

    public GetStudentsQueryValidator()
    {
        RuleFor(q => q.PageNumber)
            .GreaterThanOrEqualTo(1)
                .WithMessage("El número de página debe ser mayor o igual a 1.");

        RuleFor(q => q.PageSize)
            .GreaterThanOrEqualTo(1)
                .WithMessage("El tamaño de página debe ser mayor o igual a 1.")
            .LessThanOrEqualTo(MaxPageSize)
                .WithMessage($"El tamaño de página no puede superar {MaxPageSize} registros.");
    }
}
