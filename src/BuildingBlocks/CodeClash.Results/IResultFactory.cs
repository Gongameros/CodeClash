namespace CodeClash.Results;

public interface IResultFactory<out TResult> where TResult : Result
{
    static abstract TResult ValidationFailure(params Error[] errors);
}
