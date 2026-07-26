using FluentValidation;
using MediatR;

namespace EduApoyos.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs every registered <see cref="IValidator{TRequest}"/> before
/// the handler executes. Aggregated failures are surfaced as a single <see cref="ValidationException"/>
/// so the API layer can translate them into an RFC-7807 <c>ValidationProblemDetails</c> response.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false))
            .SelectMany(validationResult => validationResult.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}
