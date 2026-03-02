using CodeClash.AppHost.Extensions;
using CodeClash.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// Build service provider for accessing validated options
ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

var internalApiKey = builder.AddParameter("internal-api-key", secret: true);

var keycloak = builder.AddKeycloakResource();

var mongoDb = builder.AddMongoDbResource(Resources.MongoDb);

var storage = builder.AddAzureStorageResource();

var codersBlobStorage = storage.AddBlobStorageResource(Resources.CodersBlob);
var coursesBlobStorage = storage.AddBlobStorageResource(Resources.CoursesBlob);

var coders = builder.AddProject<Projects.CodeClash_Coders>(Resources.CodersService)
    .WithHttpHealthCheck("/health")
    .WithScalar("scalar", "Scalar - Coders API")
    .WaitFor(codersBlobStorage)
    .WaitFor(mongoDb)
    .WaitFor(keycloak)
    .WithReference(codersBlobStorage)
    .WithReference(mongoDb)
    .WithReference(keycloak)
    .WithEnvironment("InternalApiKey", internalApiKey);

var courses = builder.AddProject<Projects.CodeClash_Courses>(Resources.CoursesService)
    .WithHttpHealthCheck("/health")
    .WithScalar("scalar", "Scalar - Courses API")
    .WaitFor(coursesBlobStorage)
    .WaitFor(mongoDb)
    .WaitFor(keycloak)
    .WithReference(coursesBlobStorage)
    .WithReference(mongoDb)
    .WithReference(keycloak)
    .WithEnvironment("InternalApiKey", internalApiKey);

builder.AddProject<Projects.CodeClash_Gateway>(Resources.GatewayService)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithScalar("scalar/courses", "Scalar - Courses API")
    .WithScalar("scalar/coders", "Scalar - Coders API")
    .WaitFor(coders)
    .WaitFor(courses)
    .WithReference(courses)
    .WithReference(coders)
    .WithEnvironment("InternalApiKey", internalApiKey);

builder.Build().Run();
