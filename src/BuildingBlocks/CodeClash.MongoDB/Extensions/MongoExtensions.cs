using CodeClash.MongoDB.Conventions;
using CodeClash.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace CodeClash.MongoDB.Extensions;

public static class MongoExtensions
{
    public static IHostApplicationBuilder AddMongoDb(this IHostApplicationBuilder builder, string databaseName)
    {
        MongoDbConventions.RegisterConventions();

        builder.AddMongoDBClient(Resources.MongoDb);
        builder.Services.AddSingleton<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

        return builder;
    }

    public static IServiceCollection AddMongoCollection<T>(
        this IServiceCollection services, string collectionName)
    {
        services.AddSingleton<IMongoCollection<T>>(sp =>
        {
            var db = sp.GetRequiredService<IMongoDatabase>();
            return db.GetCollection<T>(collectionName);
        });

        return services;
    }
}
