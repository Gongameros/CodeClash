using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CodeClash.Utilities.Middleware;

internal sealed class InternalApiKeyMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    ILogger<InternalApiKeyMiddleware> logger)
{
    private const string HeaderName = "X-Internal-Api-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (path.Equals("/health", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/alive", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var expectedKey = configuration["InternalApiKey"];

        if (string.IsNullOrEmpty(expectedKey))
        {
            logger.LogWarning("InternalApiKey is not configured. Rejecting request.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var providedKey) ||
            string.IsNullOrEmpty(providedKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedKey.ToString());

        if (!CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }
}
