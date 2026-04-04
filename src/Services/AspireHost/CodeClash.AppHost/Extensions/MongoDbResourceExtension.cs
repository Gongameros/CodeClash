namespace CodeClash.AppHost.Extensions;

public static class MongoDbResourceExtension
{
    private const int MongoDbPort = 27017;
    private const string MongoDbVolumeName = "cc-mongo-data";

    public static IResourceBuilder<IResourceWithConnectionString> AddMongoDbResource(
        this IDistributedApplicationBuilder builder, string resourceName)
    {
        var isE2E = builder.Configuration["Testing:IsE2E"] == "true";

        if (builder.ExecutionContext.IsPublishMode && !isE2E)
        {
            // In Azure, use Cosmos DB for MongoDB — connection string provided as a parameter
            var connectionString = builder.AddParameter("mongo-connection-string", secret: true);
            return builder.AddConnectionString(resourceName, ReferenceExpression.Create($"{connectionString}"));
        }

        var mongoUsername = builder.AddParameter("mongo-username", "admin");
        var mongoPassword = builder.AddParameter("mongo-password", secret: true);

        var mongo = builder.AddMongoDB(
                name: resourceName,
                userName: mongoUsername,
                password: mongoPassword,
                port: isE2E ? null : MongoDbPort);

        if (!isE2E)
        {
            mongo.WithLifetime(ContainerLifetime.Persistent)
                .WithDataVolume(MongoDbVolumeName);
        }

        return mongo;
    }
}
