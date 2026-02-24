using CodeClash.Results;
using FluentValidation;
using Mediator;

namespace CodeClash.Courses.Shared.Validations;

public sealed class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, Result<TResponse>> where TRequest : class, IMessage
{
    public async ValueTask<Result<TResponse>> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, Result<TResponse>> next,
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

        return Result.ValidationFailure<TResponse>(errors);
    }
}
