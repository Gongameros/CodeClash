namespace CodeClash.Courses.Shared.Endpoint;

/// <summary>
///     Extension methods for IBackendEndpoint to provide common functionality
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    ///     Maps an endpoint with automatic authorization unless explicitly overridden
    /// </summary>
    public static RouteHandlerBuilder MapWithAuth(this IEndpointRouteBuilder builder, string pattern, Delegate handler,
        params string[] policies)
    {
        return MapWithAuthBase(builder.Map, pattern, handler, policies);
    }

    /// <summary>
    ///     Maps a POST endpoint with automatic authorization unless explicitly overridden
    /// </summary>
    public static RouteHandlerBuilder MapPostWithAuth(this IEndpointRouteBuilder builder, string pattern,
        Delegate handler, params string[] policies)
    {
        return MapWithAuthBase(builder.MapPost, pattern, handler, policies);
    }

    /// <summary>
    ///     Maps a GET endpoint with automatic authorization unless explicitly overridden
    /// </summary>
    public static RouteHandlerBuilder MapGetWithAuth(this IEndpointRouteBuilder builder, string pattern,
        Delegate handler, params string[] policies)
    {
        return MapWithAuthBase(builder.MapGet, pattern, handler, policies);
    }

    /// <summary>
    ///     Maps a PUT endpoint with automatic authorization unless explicitly overridden
    /// </summary>
    public static RouteHandlerBuilder MapPutWithAuth(this IEndpointRouteBuilder builder, string pattern,
        Delegate handler, params string[] policies)
    {
        return MapWithAuthBase(builder.MapPut, pattern, handler, policies);
    }

    /// <summary>
    ///     Maps a PATCH endpoint with automatic authorization unless explicitly overridden
    /// </summary>
    public static RouteHandlerBuilder MapPatchWithAuth(this IEndpointRouteBuilder builder, string pattern,
        Delegate handler, params string[] policies)
    {
        return MapWithAuthBase(builder.MapPatch, pattern, handler, policies);
    }

    /// <summary>
    ///     Maps a DELETE endpoint with automatic authorization unless explicitly overridden
    /// </summary>
    public static RouteHandlerBuilder MapDeleteWithAuth(this IEndpointRouteBuilder builder, string pattern,
        Delegate handler, params string[] policies)
    {
        return MapWithAuthBase(builder.MapDelete, pattern, handler, policies);
    }

    private static RouteHandlerBuilder MapWithAuthBase(
        Func<string, Delegate, RouteHandlerBuilder> mapFunc,
        string pattern,
        Delegate handler,
        params string[] policies)
    {
        RouteHandlerBuilder route = mapFunc(pattern, handler);

        return policies.Any()
            ? route.RequireAuthorization(policies)
            : route.RequireAuthorization();
    }
}
