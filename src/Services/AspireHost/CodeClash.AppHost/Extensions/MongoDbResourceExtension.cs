namespace CodeClash.AppHost.Extensions;

public static class MongoDbResourceExtension
{
    private const int MongoDbPort = 27017;
    private const string MongoDbVolumeName = "codeclash-mongo-data";
    private const string MongoDbResourceName = "codeclash-mongo-server";

    public static IResourceBuilder<MongoDBServerResource> AddMongoDbResource(
        this IDistributedApplicationBuilder builder)
    {
        var mongoUsername = builder.AddParameter("mongo-username", "admin");
        var mongoPassword = builder.AddParameter("mongo-password", secret: true);

        return builder.AddMongoDB(
                name: MongoDbResourceName,
                userName: mongoUsername,
                password: mongoPassword,
                port: MongoDbPort)
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume(MongoDbVolumeName);
    }
}
