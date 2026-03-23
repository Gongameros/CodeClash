using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Extensions;
using CodeClash.Courses.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace CodeClash.Courses.Tests.Infrastructure;

public sealed class IntegrationFixture : IAsyncLifetime
{
    private readonly MongoDbFixture _mongoDb = new();
    private ServiceProvider _services = null!;

    public IMediator Mediator { get; private set; } = null!;
    public IMongoCollection<Course> Courses { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _mongoDb.InitializeAsync();

        var services = new ServiceCollection();
        services.AddSingleton(_mongoDb.Client);
        services.AddSingleton(_mongoDb.Database);
        services.AddSingleton(_mongoDb.Database.GetCollection<Course>(MongoDbConstants.CoursesCollectionName));
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        services.AddValidation();

        _services = services.BuildServiceProvider();

        var scope = _services.CreateScope();
        Mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        Courses = _services.GetRequiredService<IMongoCollection<Course>>();
    }

    public Task ResetAsync() => _mongoDb.CleanupAsync();

    public async Task DisposeAsync()
    {
        await _services.DisposeAsync();
        await _mongoDb.DisposeAsync();
    }
}
