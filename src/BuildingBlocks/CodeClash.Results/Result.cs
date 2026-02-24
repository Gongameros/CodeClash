namespace CodeClash.Results;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, params Error[] errors)
    {
        if (isSuccess && errors.Length > 0)
        {
            throw new InvalidOperationException("A successful result cannot have errors.");
        }

        if (!isSuccess && errors.Length == 0)
        {
            throw new InvalidOperationException("A failed result must have at least one error.");
        }

        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// All errors associated with this result.
    /// For most failures this contains a single error.
    /// For validation failures it may contain multiple.
    /// </summary>
    public Error[] Errors { get; } = [];

    /// <summary>
    /// The first (or only) error. Convenience accessor.
    /// </summary>
    public Error FirstError => Errors.Length > 0
        ? Errors[0]
        : Error.None;

    public static Result Success() => new(true);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
    public static Result ValidationFailure(params Error[] errors) => new(false, errors);
    public static Result<TValue> ValidationFailure<TValue>(params Error[] errors) => new(default, false, errors);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="TValue"/>.
/// </summary>
public class Result<TValue> : Result
{
    internal Result(TValue? value, bool isSuccess, params Error[] errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public TValue Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}
