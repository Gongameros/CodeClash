using CodeClash.MongoDB.Indexes;
using Microsoft.Extensions.DependencyInjection;

namespace CodeClash.MongoDB.Extensions;

public static class IndexInitializerExtension
{
    public static async Task InitializeIndexesAsync(this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var initializers  = scope.ServiceProvider.GetServices<IIndexInitializer>();

        foreach (var initializer in initializers)
        {
            await initializer.CreateIndexAsync(cancellationToken);
        }
    }
}
