namespace CodeClash.Results;

/// <summary>
/// Railway-oriented programming extensions for chaining Result operations.
/// </summary>
public static class ResultFunctionalExtensions
{
    // ── Match ────────────────────────────────────────────────────────

    public static T Match<T>(this Result result, Func<T> onSuccess, Func<Error[], T> onFailure) =>
        result.IsSuccess ? onSuccess() : onFailure(result.Errors);

    public static T Match<TValue, T>(this Result<TValue> result, Func<TValue, T> onSuccess, Func<Error[], T> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Errors);

    // ── Map (transform the success value) ────────────────────────────

    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper) =>
        result.IsFailure ? Result.Failure<TOut>(result.FirstError) : Result.Success(mapper(result.Value));

    // ── Tap (side-effect without changing the result) ─────────────────

    public static Result Tap(this Result result, Action onSuccess)
    {
        if (result.IsSuccess)
        {
            onSuccess();
        }

        return result;
    }

    public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action<TValue> onSuccess)
    {
        if (result.IsSuccess)
        {
            onSuccess(result.Value);
        }

        return result;
    }

    // ── Ensure (add a validation condition) ──────────────────────────

    public static Result<TValue> Ensure<TValue>(
        this Result<TValue> result, Func<TValue, bool> predicate, Error error) =>
        result.IsFailure ? result : predicate(result.Value) ? result : Result.Failure<TValue>(error);
}
