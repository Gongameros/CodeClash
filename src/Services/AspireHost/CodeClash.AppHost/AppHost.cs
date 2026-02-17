using CodeClash.AppHost.Extensions;
using CodeClash.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// Build service provider for accessing validated options
ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

var keycloak = builder.AddKeycloakResource();

var mongoDb = builder.AddMongoDbResource();

var storage = builder.AddAzureStorageResource();

var codersBlobStorage = storage.AddBlobStorageResource(Resources.CodersBlob);
var coursesBlobStorage = storage.AddBlobStorageResource(Resources.CoursesBlob);

builder.AddProject<Projects.CodeClash_Coders>(Resources.CodersService)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithScalar()
    .WaitFor(codersBlobStorage)
    .WaitFor(mongoDb)
    .WaitFor(keycloak)
    .WithReference(codersBlobStorage)
    .WithReference(mongoDb)
    .WithReference(keycloak);

builder.AddProject<Projects.CodeClash_Courses>(Resources.CoursesService)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithScalar()
    .WaitFor(coursesBlobStorage)
    .WaitFor(mongoDb)
    .WaitFor(keycloak)
    .WithReference(coursesBlobStorage)
    .WithReference(mongoDb)
    .WithReference(keycloak);

builder.Build().Run();
