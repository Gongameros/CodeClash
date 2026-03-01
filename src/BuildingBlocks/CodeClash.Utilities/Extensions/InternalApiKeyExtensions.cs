using CodeClash.Utilities.Middleware;
using Microsoft.AspNetCore.Builder;

namespace CodeClash.Utilities.Extensions;

public static class InternalApiKeyExtensions
{
    public static IApplicationBuilder UseInternalApiKey(this IApplicationBuilder app)
    {
        return app.UseMiddleware<InternalApiKeyMiddleware>();
    }
}
