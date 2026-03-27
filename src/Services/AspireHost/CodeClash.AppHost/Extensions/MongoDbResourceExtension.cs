namespace CodeClash.AppHost.Extensions;

public static class MongoDbResourceExtension
{
    private const int MongoDbPort = 27017;
    private const string MongoDbVolumeName = "codeclash-mongo-data";

    public static IResourceBuilder<MongoDBServerResource> AddMongoDbResource(
        this IDistributedApplicationBuilder builder, string resourceName)
    {
        var mongoUsername = builder.AddParameter("mongo-username", "admin");
        var mongoPassword = builder.AddParameter("mongo-password", secret: true);

        var isE2E = builder.Configuration["Testing:IsE2E"] == "true";

        var mongo = builder.AddMongoDB(
                name: resourceName,
                userName: mongoUsername,
                password: mongoPassword,
                port: isE2E ? null : MongoDbPort);

        if (!isE2E)
        {
            mongo
                .WithLifetime(ContainerLifetime.Persistent)
                .WithDataVolume(MongoDbVolumeName);
        }

        return mongo;
    }
}
