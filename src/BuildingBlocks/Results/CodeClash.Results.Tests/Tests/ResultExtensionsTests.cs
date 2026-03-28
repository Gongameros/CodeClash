using Microsoft.AspNetCore.Http.HttpResults;
using Shouldly;

namespace CodeClash.Results.Tests.Tests;

public sealed class ResultExtensionsTests
{
    // ── ToProblemDetails (non-generic) ──────────────────────────────

    [Fact]
    public void ToProblemDetails_Success_ReturnsOk()
    {
        var result = Result.Success();

        var response = result.ToProblemDetails();

        response.ShouldBeOfType<Ok>();
    }

    [Fact]
    public void ToProblemDetails_Failure_ReturnsProblem()
    {
        var result = Result.Failure(Error.Failure("E", "fail"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(500);
    }

    [Fact]
    public void ToProblemDetails_NotFound_Returns404()
    {
        var result = Result.Failure(Error.NotFound("NF", "missing"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void ToProblemDetails_Validation_Returns400()
    {
        var result = Result.Failure(Error.Validation("V", "invalid"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(400);
    }

    [Fact]
    public void ToProblemDetails_Unauthorized_Returns401()
    {
        var result = Result.Failure(Error.Unauthorized("A", "denied"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(401);
    }

    [Fact]
    public void ToProblemDetails_Forbidden_Returns403()
    {
        var result = Result.Failure(Error.Forbidden("F", "no access"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(403);
    }

    [Fact]
    public void ToProblemDetails_Conflict_Returns409()
    {
        var result = Result.Failure(Error.Conflict("C", "duplicate"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(409);
    }

    [Fact]
    public void ToProblemDetails_Unavailable_Returns503()
    {
        var result = Result.Failure(Error.Unavailable("U", "down"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(503);
    }

    // ── ToProblemDetails (generic) ──────────────────────────────────

    [Fact]
    public void ToProblemDetails_GenericSuccess_ReturnsOkWithValue()
    {
        var result = Result.Success(42);

        var response = result.ToProblemDetails();

        var ok = response.ShouldBeOfType<Ok<int>>();
        ok.Value.ShouldBe(42);
    }

    [Fact]
    public void ToProblemDetails_GenericFailure_ReturnsProblem()
    {
        var result = Result.Failure<string>(Error.NotFound("NF", "missing"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(404);
    }

    // ── ToCreatedProblemDetails ──────────────────────────────────────

    [Fact]
    public void ToCreatedProblemDetails_Success_ReturnsCreated()
    {
        var result = Result.Success("new-item");

        var response = result.ToCreatedProblemDetails("/api/items/1");

        var created = response.ShouldBeOfType<Created<string>>();
        created.Location.ShouldBe("/api/items/1");
        created.Value.ShouldBe("new-item");
    }

    [Fact]
    public void ToCreatedProblemDetails_Failure_ReturnsProblem()
    {
        var result = Result.Failure<string>(Error.Validation("V", "invalid"));

        var response = result.ToCreatedProblemDetails("/api/items/1");

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(400);
    }

    // ── ToNoContentProblemDetails ────────────────────────────────────

    [Fact]
    public void ToNoContentProblemDetails_Success_ReturnsNoContent()
    {
        var result = Result.Success();

        var response = result.ToNoContentProblemDetails();

        response.ShouldBeOfType<NoContent>();
    }

    [Fact]
    public void ToNoContentProblemDetails_Failure_ReturnsProblem()
    {
        var result = Result.Failure(Error.Failure("E", "fail"));

        var response = result.ToNoContentProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(500);
    }

    // ── Multiple validation errors ──────────────────────────────────

    [Fact]
    public void ToProblemDetails_MultipleValidationErrors_Returns400WithErrorsArray()
    {
        var e1 = Error.Validation("V1", "required");
        var e2 = Error.Validation("V2", "too long");
        var result = Result.ValidationFailure(e1, e2);

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(400);
        problem.ProblemDetails.Extensions.ShouldContainKey("errors");
    }

    [Fact]
    public void ToProblemDetails_SingleError_HasCodeExtension()
    {
        var error = Error.Failure("ERR_001", "fail");
        var result = Result.Failure(error);

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.ProblemDetails.Extensions.ShouldContainKey("code");
        problem.ProblemDetails.Extensions["code"].ShouldBe("ERR_001");
    }

    [Fact]
    public void ToProblemDetails_SingleError_HasDetailDescription()
    {
        var error = Error.Failure("ERR", "something broke");
        var result = Result.Failure(error);

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.ProblemDetails.Detail.ShouldBe("something broke");
    }

    [Fact]
    public void ToProblemDetails_Failure_HasRfcTypeUrl()
    {
        var result = Result.Failure(Error.NotFound("NF", "missing"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.ProblemDetails.Type.ShouldContain("rfc9110");
    }

    [Fact]
    public void ToProblemDetails_Failure_HasTitle()
    {
        var result = Result.Failure(Error.NotFound("NF", "missing"));

        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.ProblemDetails.Title.ShouldBe("Not Found");
    }

    // ── Async overloads ─────────────────────────────────────────────

    [Fact]
    public async Task ToProblemDetails_Async_Success_ReturnsOk()
    {
        var task = Task.FromResult(Result.Success());

        var response = await task.ToProblemDetails();

        response.ShouldBeOfType<Ok>();
    }

    [Fact]
    public async Task ToProblemDetails_Async_Failure_ReturnsProblem()
    {
        var task = Task.FromResult(Result.Failure(Error.Failure("E", "fail")));

        var response = await task.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(500);
    }

    [Fact]
    public async Task ToProblemDetails_AsyncGeneric_Success_ReturnsOk()
    {
        var task = Task.FromResult(Result.Success(42));

        var response = await task.ToProblemDetails();

        var ok = response.ShouldBeOfType<Ok<int>>();
        ok.Value.ShouldBe(42);
    }

    [Fact]
    public async Task ToProblemDetails_AsyncGeneric_Failure_ReturnsProblem()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("E", "fail")));

        var response = await task.ToProblemDetails();

        response.ShouldBeOfType<ProblemHttpResult>();
    }

    [Fact]
    public async Task ToCreatedProblemDetails_Async_Success_ReturnsCreated()
    {
        var task = Task.FromResult(Result.Success("item"));

        var response = await task.ToCreatedProblemDetails("/api/items/1");

        var created = response.ShouldBeOfType<Created<string>>();
        created.Value.ShouldBe("item");
    }

    [Fact]
    public async Task ToCreatedProblemDetails_Async_Failure_ReturnsProblem()
    {
        var task = Task.FromResult(Result.Failure<string>(Error.Conflict("C", "dup")));

        var response = await task.ToCreatedProblemDetails("/api/items/1");

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(409);
    }

    [Fact]
    public async Task ToNoContentProblemDetails_Async_Success_ReturnsNoContent()
    {
        var task = Task.FromResult(Result.Success());

        var response = await task.ToNoContentProblemDetails();

        response.ShouldBeOfType<NoContent>();
    }

    [Fact]
    public async Task ToNoContentProblemDetails_Async_Failure_ReturnsProblem()
    {
        var task = Task.FromResult(Result.Failure(Error.Unauthorized("A", "denied")));

        var response = await task.ToNoContentProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(401);
    }

    // ── Title mapping for all error types ───────────────────────────

    [Theory]
    [InlineData(ErrorType.Validation, "Bad Request")]
    [InlineData(ErrorType.Unauthorized, "Unauthorized")]
    [InlineData(ErrorType.Forbidden, "Forbidden")]
    [InlineData(ErrorType.NotFound, "Not Found")]
    [InlineData(ErrorType.Conflict, "Conflict")]
    [InlineData(ErrorType.Unavailable, "Service Unavailable")]
    [InlineData(ErrorType.Failure, "Internal Server Error")]
    public void ToProblemDetails_ErrorType_MapsToCorrectTitle(ErrorType type, string expectedTitle)
    {
        var error = type switch
        {
            ErrorType.Validation => Error.Validation("C", "d"),
            ErrorType.Unauthorized => Error.Unauthorized("C", "d"),
            ErrorType.Forbidden => Error.Forbidden("C", "d"),
            ErrorType.NotFound => Error.NotFound("C", "d"),
            ErrorType.Conflict => Error.Conflict("C", "d"),
            ErrorType.Unavailable => Error.Unavailable("C", "d"),
            _ => Error.Failure("C", "d")
        };

        var result = Result.Failure(error);
        var response = result.ToProblemDetails();

        var problem = response.ShouldBeOfType<ProblemHttpResult>();
        problem.ProblemDetails.Title.ShouldBe(expectedTitle);
    }
}
