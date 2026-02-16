using CodeClash.AppHost.Extensions;
using CodeClash.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// Build service provider for accessing validated options
ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

var mongoDb = builder.AddMongoDbResource();

var storage = builder.AddAzureStorageResource();
var codersBlobStorage = storage.AddBlobStorageResource(Resources.CodersBlob);
var coursesBlobStorage = storage.AddBlobStorageResource(Resources.CoursesBlob);

builder.AddProject<Projects.CodeClash_Coders>(Resources.CodersService)
    .WaitFor(codersBlobStorage)
    .WaitFor(mongoDb)
    .WithReference(codersBlobStorage)
    .WithReference(mongoDb);

builder.AddProject<Projects.CodeClash_Courses>(Resources.CoursesService)
    .WaitFor(coursesBlobStorage)
    .WaitFor(mongoDb)
    .WithReference(coursesBlobStorage)
    .WithReference(mongoDb);

builder.Build().Run();
