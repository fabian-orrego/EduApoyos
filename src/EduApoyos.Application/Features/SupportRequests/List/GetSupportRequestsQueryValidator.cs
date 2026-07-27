using FluentValidation;

namespace EduApoyos.Application.Features.SupportRequests.List;

/// <summary>
/// FluentValidation rules for <see cref="GetSupportRequestsQuery"/> (US-015). Enforces the
/// mandatory pagination parameters, the project-wide 100 items page cap and the coherence
/// between the date range filters (from ≤ to).
/// </summary>
public sealed class GetSupportRequestsQueryValidator
    : AbstractValidator<GetSupportRequestsQuery>
{
    public const int MaxPageSize = 100;

    public GetSupportRequestsQueryValidator()
    {
        RuleFor(q => q.PageNumber)
            .GreaterThanOrEqualTo(1)
                .WithMessage("El número de página debe ser mayor o igual a 1.");

        RuleFor(q => q.PageSize)
            .GreaterThanOrEqualTo(1)
                .WithMessage("El tamaño de página debe ser mayor o igual a 1.")
            .LessThanOrEqualTo(MaxPageSize)
                .WithMessage($"El tamaño de página no puede superar {MaxPageSize} registros.");

        RuleFor(q => q.Status!.Value)
            .IsInEnum().WithMessage("El estado no es válido.")
            .When(q => q.Status.HasValue);

        RuleFor(q => q.SupportType!.Value)
            .IsInEnum().WithMessage("El tipo de apoyo no es válido.")
            .When(q => q.SupportType.HasValue);

        RuleFor(q => q)
            .Must(q => q.FromDate!.Value.Date <= q.ToDate!.Value.Date)
            .WithMessage("La fecha inicial no puede ser posterior a la fecha final.")
            .When(q => q.FromDate.HasValue && q.ToDate.HasValue);
    }
}
