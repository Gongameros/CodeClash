using Shouldly;

namespace CodeClash.Results.Tests.Tests;

public sealed class ResultTests
{
    // ── Success ─────────────────────────────────────────────────────

    [Fact]
    public void Success_IsSuccess_ReturnsTrue()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
    }

    [Fact]
    public void Success_Errors_IsEmpty()
    {
        var result = Result.Success();

        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Success_FirstError_ReturnsNone()
    {
        var result = Result.Success();

        result.FirstError.ShouldBe(Error.None);
    }

    // ── Failure ─────────────────────────────────────────────────────

    [Fact]
    public void Failure_IsFailure_ReturnsTrue()
    {
        var error = Error.Failure("E", "fail");
        var result = Result.Failure(error);

        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void Failure_ContainsError()
    {
        var error = Error.NotFound("NF", "not found");
        var result = Result.Failure(error);

        result.Errors.Length.ShouldBe(1);
        result.FirstError.ShouldBe(error);
    }

    // ── ValidationFailure ───────────────────────────────────────────

    [Fact]
    public void ValidationFailure_SingleError_Works()
    {
        var error = Error.Validation("V1", "required");
        var result = Result.ValidationFailure(error);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Length.ShouldBe(1);
        result.FirstError.ShouldBe(error);
    }

    [Fact]
    public void ValidationFailure_MultipleErrors_ContainsAll()
    {
        var e1 = Error.Validation("V1", "required");
        var e2 = Error.Validation("V2", "too short");
        var e3 = Error.Validation("V3", "invalid format");

        var result = Result.ValidationFailure(e1, e2, e3);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Length.ShouldBe(3);
        result.FirstError.ShouldBe(e1);
        result.Errors[1].ShouldBe(e2);
        result.Errors[2].ShouldBe(e3);
    }

    // ── Invariant protection ────────────────────────────────────────

    [Fact]
    public void Success_FactoryMethod_ProducesValidSuccessResult()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Failure_WithoutError_CannotBeCreated()
    {
        // ValidationFailure with empty array should throw
        Should.Throw<InvalidOperationException>(
            () => Result.ValidationFailure());
    }

    // ── Generic Success ─────────────────────────────────────────────

    [Fact]
    public void GenericSuccess_HasValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void GenericSuccess_StringValue()
    {
        var result = Result.Success("hello");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("hello");
    }

    [Fact]
    public void GenericSuccess_ComplexObject()
    {
        var obj = new { Id = 1, Name = "test" };
        var result = Result.Success(obj);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(obj);
    }

    // ── Generic Failure ─────────────────────────────────────────────

    [Fact]
    public void GenericFailure_AccessingValue_ThrowsInvalidOperation()
    {
        var error = Error.Failure("E", "fail");
        var result = Result.Failure<int>(error);

        result.IsFailure.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void GenericFailure_ContainsError()
    {
        var error = Error.NotFound("NF", "missing");
        var result = Result.Failure<string>(error);

        result.FirstError.ShouldBe(error);
        result.Errors.Length.ShouldBe(1);
    }

    // ── Generic ValidationFailure ───────────────────────────────────

    [Fact]
    public void GenericValidationFailure_MultipleErrors()
    {
        var e1 = Error.Validation("V1", "err1");
        var e2 = Error.Validation("V2", "err2");

        var result = Result.ValidationFailure<int>(e1, e2);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Length.ShouldBe(2);
        Should.Throw<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void GenericValidationFailure_EmptyErrors_Throws()
    {
        Should.Throw<InvalidOperationException>(
            () => Result.ValidationFailure<int>());
    }

    // ── Implicit operator ───────────────────────────────────────────

    [Fact]
    public void ImplicitOperator_NonNullValue_CreatesSuccess()
    {
        Result<string> result = "hello";

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("hello");
    }

    [Fact]
    public void ImplicitOperator_NullValue_CreatesFailureWithNullValueError()
    {
        Result<string> result = (string?)null;

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBe(Error.NullValue);
    }

    [Fact]
    public void ImplicitOperator_IntValue_CreatesSuccess()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    // ── IsFailure / IsSuccess consistency ───────────────────────────

    [Fact]
    public void IsFailure_IsAlwaysOppositeOfIsSuccess()
    {
        var success = Result.Success();
        var failure = Result.Failure(Error.Failure("E", "d"));

        success.IsFailure.ShouldBe(!success.IsSuccess);
        failure.IsFailure.ShouldBe(!failure.IsSuccess);
    }

    // ── IResultFactory ──────────────────────────────────────────────

    [Fact]
    public void Result_ImplementsIResultFactory()
    {
        typeof(IResultFactory<Result>).IsAssignableFrom(typeof(Result)).ShouldBeTrue();
    }

    [Fact]
    public void GenericResult_ImplementsIResultFactory()
    {
        typeof(IResultFactory<Result<int>>).IsAssignableFrom(typeof(Result<int>)).ShouldBeTrue();
    }

    // ── Result<TValue> inherits from Result ─────────────────────────

    [Fact]
    public void GenericResult_InheritsFromResult()
    {
        var result = Result.Success(42);

        result.ShouldBeAssignableTo<Result>();
    }
}
