using CodeClash.Courses.Shared.Constants;
using CodeClash.MongoDB.Conventions;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace CodeClash.Courses.Tests.Infrastructure;

public sealed class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder().Build();

    public IMongoClient Client { get; private set; } = null!;
    public IMongoDatabase Database { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        MongoDbConventions.RegisterConventions();
        Client = new MongoClient(_container.GetConnectionString());
        Database = Client.GetDatabase(MongoDbConstants.DatabaseName);
    }

    public async Task CleanupAsync()
    {
        var cursor = await Database.ListCollectionNamesAsync();
        var names = await cursor.ToListAsync();
        foreach (var name in names)
            await Database.DropCollectionAsync(name);
    }

    public async Task DisposeAsync() => await _container.StopAsync();
}
