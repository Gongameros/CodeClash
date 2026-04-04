using Shouldly;

namespace CodeClash.Results.Tests.Tests;

public sealed class ResultFunctionalExtensionsTests
{
    // ── Match (non-generic) ─────────────────────────────────────────

    [Fact]
    public void Match_Success_CallsOnSuccess()
    {
        var result = Result.Success();

        var output = result.Match(
            () => "ok",
            errors => $"fail: {errors.Length}");

        output.ShouldBe("ok");
    }

    [Fact]
    public void Match_Failure_CallsOnFailure()
    {
        var error = Error.Failure("E", "fail");
        var result = Result.Failure(error);

        var output = result.Match(
            () => "ok",
            errors => $"fail: {errors[0].Code}");

        output.ShouldBe("fail: E");
    }

    [Fact]
    public void Match_Failure_PassesAllErrors()
    {
        var e1 = Error.Validation("V1", "a");
        var e2 = Error.Validation("V2", "b");
        var result = Result.ValidationFailure(e1, e2);

        var count = result.Match(
            () => 0,
            errors => errors.Length);

        count.ShouldBe(2);
    }

    // ── Match (generic) ─────────────────────────────────────────────

    [Fact]
    public void Match_GenericSuccess_CallsOnSuccessWithValue()
    {
        var result = Result.Success(42);

        var output = result.Match(
            value => $"got: {value}",
            errors => "fail");

        output.ShouldBe("got: 42");
    }

    [Fact]
    public void Match_GenericFailure_CallsOnFailure()
    {
        var error = Error.NotFound("NF", "missing");
        var result = Result.Failure<int>(error);

        var output = result.Match(
            value => "ok",
            errors => errors[0].Description);

        output.ShouldBe("missing");
    }

    // ── Map ─────────────────────────────────────────────────────────

    [Fact]
    public void Map_Success_TransformsValue()
    {
        var result = Result.Success(5);

        var mapped = result.Map(v => v * 2);

        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(10);
    }

    [Fact]
    public void Map_Success_CanChangeType()
    {
        var result = Result.Success(42);

        var mapped = result.Map(v => v.ToString());

        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("42");
    }

    [Fact]
    public void Map_Failure_PropagatesError()
    {
        var error = Error.Failure("E", "fail");
        var result = Result.Failure<int>(error);

        var mapped = result.Map(v => v * 2);

        mapped.IsFailure.ShouldBeTrue();
        mapped.FirstError.ShouldBe(error);
    }

    [Fact]
    public void Map_Failure_DoesNotCallMapper()
    {
        var result = Result.Failure<int>(Error.Failure("E", "fail"));
        var called = false;

        result.Map(v => { called = true; return v; });

        called.ShouldBeFalse();
    }

    [Fact]
    public void Map_CanChain()
    {
        var result = Result.Success(2);

        var mapped = result
            .Map(v => v * 3)
            .Map(v => v + 1)
            .Map(v => v.ToString());

        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("7");
    }

    // ── Tap (non-generic) ───────────────────────────────────────────

    [Fact]
    public void Tap_Success_ExecutesSideEffect()
    {
        var result = Result.Success();
        var executed = false;

        var returned = result.Tap(() => executed = true);

        executed.ShouldBeTrue();
        returned.ShouldBeSameAs(result);
    }

    [Fact]
    public void Tap_Failure_DoesNotExecuteSideEffect()
    {
        var result = Result.Failure(Error.Failure("E", "fail"));
        var executed = false;

        var returned = result.Tap(() => executed = true);

        executed.ShouldBeFalse();
        returned.ShouldBeSameAs(result);
    }

    // ── Tap (generic) ───────────────────────────────────────────────

    [Fact]
    public void Tap_GenericSuccess_ExecutesSideEffectWithValue()
    {
        var result = Result.Success(42);
        var capturedValue = 0;

        var returned = result.Tap(v => capturedValue = v);

        capturedValue.ShouldBe(42);
        returned.ShouldBeSameAs(result);
    }

    [Fact]
    public void Tap_GenericFailure_DoesNotExecuteSideEffect()
    {
        var result = Result.Failure<int>(Error.Failure("E", "fail"));
        var executed = false;

        var returned = result.Tap(v => executed = true);

        executed.ShouldBeFalse();
        returned.ShouldBeSameAs(result);
    }

    // ── Ensure ──────────────────────────────────────────────────────

    [Fact]
    public void Ensure_Success_PredicateTrue_ReturnsOriginalResult()
    {
        var result = Result.Success(10);
        var error = Error.Validation("V", "must be positive");

        var ensured = result.Ensure(v => v > 0, error);

        ensured.IsSuccess.ShouldBeTrue();
        ensured.Value.ShouldBe(10);
        ensured.ShouldBeSameAs(result);
    }

    [Fact]
    public void Ensure_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success(-5);
        var error = Error.Validation("V", "must be positive");

        var ensured = result.Ensure(v => v > 0, error);

        ensured.IsFailure.ShouldBeTrue();
        ensured.FirstError.ShouldBe(error);
    }

    [Fact]
    public void Ensure_Failure_DoesNotEvaluatePredicate()
    {
        var result = Result.Failure<int>(Error.Failure("E", "fail"));
        var error = Error.Validation("V", "irrelevant");
        var called = false;

        var ensured = result.Ensure(v => { called = true; return v > 0; }, error);

        called.ShouldBeFalse();
        ensured.IsFailure.ShouldBeTrue();
        ensured.ShouldBeSameAs(result);
    }

    [Fact]
    public void Ensure_CanChain()
    {
        var result = Result.Success(50);

        var ensured = result
            .Ensure(v => v > 0, Error.Validation("V1", "positive"))
            .Ensure(v => v < 100, Error.Validation("V2", "under 100"));

        ensured.IsSuccess.ShouldBeTrue();
        ensured.Value.ShouldBe(50);
    }

    [Fact]
    public void Ensure_ChainStopsAtFirstFailure()
    {
        var result = Result.Success(150);
        var secondCalled = false;

        var ensured = result
            .Ensure(v => v < 100, Error.Validation("V1", "under 100"))
            .Ensure(v => { secondCalled = true; return v > 0; }, Error.Validation("V2", "positive"));

        ensured.IsFailure.ShouldBeTrue();
        ensured.FirstError.Code.ShouldBe("V1");
        secondCalled.ShouldBeFalse();
    }

    // ── Composing Map + Ensure + Tap + Match ────────────────────────

    [Fact]
    public void FullPipeline_Success()
    {
        var sideEffect = "";

        var output = Result.Success(5)
            .Ensure(v => v > 0, Error.Validation("V", "positive"))
            .Map(v => v * 10)
            .Tap(v => sideEffect = $"value={v}")
            .Match(
                v => $"ok: {v}",
                errors => $"fail: {errors[0].Code}");

        output.ShouldBe("ok: 50");
        sideEffect.ShouldBe("value=50");
    }

    [Fact]
    public void FullPipeline_Failure_ShortCircuits()
    {
        var mapCalled = false;
        var tapCalled = false;

        var output = Result.Success(-1)
            .Ensure(v => v > 0, Error.Validation("NEG", "must be positive"))
            .Map(v => { mapCalled = true; return v * 10; })
            .Tap(v => tapCalled = true)
            .Match(
                v => "ok",
                errors => errors[0].Code);

        output.ShouldBe("NEG");
        mapCalled.ShouldBeFalse();
        tapCalled.ShouldBeFalse();
    }
}
