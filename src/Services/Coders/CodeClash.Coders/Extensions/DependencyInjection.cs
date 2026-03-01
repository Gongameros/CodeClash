using CodeClash.MongoDB.Extensions;

namespace CodeClash.Coders.Extensions;

public static class DependencyInjection
{
    private const string DatabaseName = "coders-db";
    public static IHostApplicationBuilder AddDependencyInjection(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        builder.Services.AddValidation();
        return builder;
    }

    public static IHostApplicationBuilder AddMongoDb(this IHostApplicationBuilder builder)
    {
        builder.AddMongoDb(DatabaseName);

        return builder;
    }
}
