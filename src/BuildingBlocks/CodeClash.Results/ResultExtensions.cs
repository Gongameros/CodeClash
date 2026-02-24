using Microsoft.AspNetCore.Http;

namespace CodeClash.Results;

/// <summary>
/// Extension methods that convert Result / Result{TValue}
/// into minimal API IResult responses using RFC 9457 Problem Details.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a Result to an IResult. Success → 200 OK, Failure → ProblemDetails.
    /// </summary>
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Ok();
        }

        return CreateProblemResult(result);
    }

    /// <summary>
    /// Maps a typed Result to an IResult. Success → 200 OK with value, Failure → ProblemDetails.
    /// </summary>
    public static IResult ToProblemDetails<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Ok(result.Value);
        }

        return CreateProblemResult(result);
    }

    /// <summary>
    /// Maps a typed Result to a 201 Created response on success.
    /// </summary>
    public static IResult ToCreatedProblemDetails<TValue>(this Result<TValue> result, string uri)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Created(uri, result.Value);
        }

        return CreateProblemResult(result);
    }

    /// <summary>
    /// Maps a Result to 204 No Content on success.
    /// </summary>
    public static IResult ToNoContentProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.NoContent();
        }

        return CreateProblemResult(result);
    }

    // ── Async overloads ──────────────────────────────────────────────

    public static async Task<IResult> ToProblemDetails(this Task<Result> resultTask) =>
        (await resultTask).ToProblemDetails();

    public static async Task<IResult> ToProblemDetails<TValue>(this Task<Result<TValue>> resultTask) =>
        (await resultTask).ToProblemDetails();

    public static async Task<IResult> ToCreatedProblemDetails<TValue>(this Task<Result<TValue>> resultTask, string uri) =>
        (await resultTask).ToCreatedProblemDetails(uri);

    public static async Task<IResult> ToNoContentProblemDetails(this Task<Result> resultTask) =>
        (await resultTask).ToNoContentProblemDetails();

    // ── Internal helpers ─────────────────────────────────────────────

    private static IResult CreateProblemResult(Result result)
    {
        var firstError = result.FirstError;
        int statusCode = MapToStatusCode(firstError.Type);

        // Validation with multiple errors → include full error array
        if (firstError.Type == ErrorType.Validation && result.Errors.Length > 1)
        {
            return Microsoft.AspNetCore.Http.Results.Problem(
                statusCode: statusCode,
                title: GetTitle(firstError.Type),
                type: GetRfcType(statusCode),
                detail: "One or more validation errors occurred.",
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = result.Errors.Select(e => new
                    {
                        code = e.Code,
                        description = e.Description
                    }).ToArray()
                });
        }

        // Single error → simple problem details
        return Microsoft.AspNetCore.Http.Results.Problem(
            statusCode: statusCode,
            title: GetTitle(firstError.Type),
            type: GetRfcType(statusCode),
            detail: firstError.Description,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = firstError.Code
            });
    }

    private static int MapToStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation   => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden    => StatusCodes.Status403Forbidden,
        ErrorType.NotFound     => StatusCodes.Status404NotFound,
        ErrorType.Conflict     => StatusCodes.Status409Conflict,
        ErrorType.Unavailable  => StatusCodes.Status503ServiceUnavailable,
        _                      => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation   => "Bad Request",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden    => "Forbidden",
        ErrorType.NotFound     => "Not Found",
        ErrorType.Conflict     => "Conflict",
        ErrorType.Unavailable  => "Service Unavailable",
        _                      => "Internal Server Error"
    };

    private static string GetRfcType(int statusCode) =>
        $"https://tools.ietf.org/html/rfc9110#section-15.{GetRfcSection(statusCode)}";

    private static string GetRfcSection(int statusCode) => statusCode switch
    {
        400 => "5.1",
        401 => "5.2",
        403 => "5.4",
        404 => "5.5",
        409 => "5.10",
        500 => "6.1",
        503 => "6.4",
        _   => "6.1"
    };
}
