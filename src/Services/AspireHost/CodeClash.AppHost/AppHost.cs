using Azure.Provisioning.AppContainers;
using CodeClash.AppHost.Extensions;
using CodeClash.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// Build service provider for accessing validated options
ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

string normalisedEnvName = builder.Environment.EnvironmentName.ToLowerInvariant();
builder.AddAzureContainerAppEnvironment("cc");
// var internalApiKey = builder.AddParameter("internal-api-key", secret: true);

// Keycloak is only managed by Aspire locally; deployed externally in Azure
var keycloak = builder.AddKeycloakResource();

var mongoDb = builder.AddMongoDbResource(Resources.MongoDb);

// var storage = builder.AddAzureStorageResource();
//
// var codersBlobStorage = storage.AddBlobStorageResource(Resources.CodersBlob);
// var coursesBlobStorage = storage.AddBlobStorageResource(Resources.CoursesBlob);
//
// var coders = builder.AddProject<Projects.CodeClash_Coders>(Resources.CodersService)
//     .WithHttpHealthCheck("/health")
//     .WithScalar("scalar", "Scalar - Coders API")
//     .WaitFor(codersBlobStorage)
//     .WaitFor(mongoDb)
//     .WaitFor(keycloak)
//     .WithReference(codersBlobStorage)
//     .WithReference(mongoDb)
//     .WithReference(keycloak)
//     .WithEnvironment("InternalApiKey", internalApiKey);

var courses = builder.AddProject<Projects.CodeClash_Courses>(Resources.CoursesService)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithScalar("scalar", "Scalar - Courses API")
    // .WaitFor(coursesBlobStorage)
    .WaitFor(mongoDb)
    // .WithReference(coursesBlobStorage)
    .WithReference(mongoDb)
    .WithEnvironment("Keycloak__Realm", "codeclash")
    .WithEnvironment("Keycloak__Audience", "codeclash-api")
    .PublishAsAzureContainerApp((infrastructure, app) =>
    {
        app.Template.Scale.MinReplicas = 0;
        app.Template.Scale.MaxReplicas = 2;
        app.Template.Scale.Rules.Add(new ContainerAppScaleRule
        {
            Name = "http-scaling",
            Http = new ContainerAppHttpScaleRule
            {
                Metadata = { { "concurrentRequests", "100" } }
            }
        });
    });
    // .WithEnvironment("InternalApiKey", internalApiKey);

// builder.AddProject<Projects.CodeClash_Gateway>(Resources.GatewayService)
//     .WithExternalHttpEndpoints()
//     .WithHttpHealthCheck("/health")
//     .WithScalar("scalar/courses", "Scalar - Courses API")
//     .WithScalar("scalar/coders", "Scalar - Coders API")
//     .WaitFor(coders)
//     .WaitFor(courses)
//     .WithReference(courses)
//     .WithReference(coders)
//     .WithEnvironment("InternalApiKey", internalApiKey);

var keycloakClientSecret = builder.AddParameter("keycloak-client-secret", secret: true);

var web = builder.AddProject<Projects.CodeClash_Web>(Resources.WebService)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WaitFor(courses)
    .WithReference(courses)
    .WithEnvironment("Keycloak__Realm", "codeclash")
    .WithEnvironment("Keycloak__ClientId", "codeclash-web")
    .WithEnvironment("Keycloak__ClientSecret", keycloakClientSecret)
    .PublishAsAzureContainerApp((infrastructure, app) =>
    {
        app.Template.Scale.MinReplicas = 0;
        app.Template.Scale.MaxReplicas = 2;
        app.Configuration.Ingress.StickySessionsAffinity = StickySessionAffinity.Sticky;
        app.Template.Scale.Rules.Add(new ContainerAppScaleRule
        {
            Name = "http-scaling",
            Http = new ContainerAppHttpScaleRule
            {
                Metadata = { { "concurrentRequests", "100" } }
            }
        });
    });

if (keycloak is not null)
{
    courses.WaitFor(keycloak).WithReference(keycloak);
    web.WaitFor(keycloak).WithReference(keycloak);
}
else
{
    // In publish mode, Keycloak is external — pass URL directly, bypassing service discovery
    // e.g. "https://keycloakwebapp-xxx.azurewebsites.net"
    var keycloakBaseUrl = builder.AddParameter("keycloak-base-url");
    courses.WithEnvironment("Keycloak__BaseUrl", keycloakBaseUrl);
    web.WithEnvironment("Keycloak__BaseUrl", keycloakBaseUrl);
    web.WithEnvironment("CODECLASH_KEYCLOAK_HTTPS", keycloakBaseUrl);
}

builder.Build().Run();
