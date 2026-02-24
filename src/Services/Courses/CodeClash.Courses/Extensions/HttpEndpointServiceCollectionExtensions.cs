using CodeClash.Courses.Shared.Endpoint;
using ServiceScan.SourceGenerator;

namespace CodeClash.Courses.Extensions;

public static partial class HttpEndpointServiceCollectionExtensions
{
    /// <summary>
    /// Register all endpoints that implement the <see cref="IEndpoint"/> interface, no reflection used.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    [GenerateServiceRegistrations(AssignableTo = typeof(IEndpoint), CustomHandler = nameof(MapEndpoint))]
    public static partial IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder);

    private static void MapEndpoint<T>(IEndpointRouteBuilder builder) where T : IEndpoint
    {
        T.MapEndpoint(builder);
    }
}
