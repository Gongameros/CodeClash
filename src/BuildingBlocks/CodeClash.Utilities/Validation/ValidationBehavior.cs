using CodeClash.Results;
using FluentValidation;
using Mediator;

namespace CodeClash.Utilities.Validation;

public sealed class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IMessage
    where TResponse : Result, IResultFactory<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validator is null)
        {
            return await next(message, cancellationToken);
        }

        var validationResult = await validator.ValidateAsync(message, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next(message, cancellationToken);
        }

        var errors = validationResult.Errors
            .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
            .ToArray();

        return TResponse.ValidationFailure(errors);
    }
}
