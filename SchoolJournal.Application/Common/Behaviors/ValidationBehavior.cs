using ErrorOr;
using FluentValidation;
using MediatR;

namespace SchoolJournal.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        if (!validators.Any())
        {
#pragma warning disable CA2016 // MediatR delegate does not accept cancellation token
            return await next().ConfigureAwait(false);
#pragma warning restore CA2016
        }

        var context = new ValidationContext<TRequest>(request);

        var validationFailures = await Task.WhenAll(
                    validators.Select(validator => validator.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

        var errors = validationFailures
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure is not null)
            .Select(failure => Error.Validation(
                code: failure.PropertyName,
                description: failure.ErrorMessage))
            .ToList();

        if (errors.Count != 0)
        {
            return (dynamic)errors;
        }

#pragma warning disable CA2016 // MediatR delegate does not accept cancellation token
        return await next().ConfigureAwait(false);
#pragma warning restore CA2016
    }
}