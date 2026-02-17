using Aspire.Hosting.Azure;
using Azure.Provisioning.Storage;
using CodeClash.Shared.Constants;

namespace CodeClash.AppHost.Extensions;

public static class AzureStorageResourceExtension
{
    private const int BlobPort = 10000;

    public static IResourceBuilder<AzureBlobStorageResource> AddBlobStorageResource(
        this IResourceBuilder<AzureStorageResource> storage, string blobStorageName)
    {
        return storage.AddBlobs(blobStorageName);
    }

    public static IResourceBuilder<AzureStorageResource> AddAzureStorageResource(
        this IDistributedApplicationBuilder builder)
    {
        var storage = builder.AddAzureStorage(Resources.Storage)
            .ConfigureInfrastructure(infra =>
            {
                var storageAccount = infra
                    .GetProvisionableResources()
                    .OfType<StorageAccount>()
                    .Single();

                storageAccount.AllowCrossTenantReplication = false;
            });

        if (builder.ExecutionContext.IsRunMode)
        {
            storage.RunAsEmulator(configure => configure
                .WithBlobPort(BlobPort)
                .WithDataBindMount()
                .WithLifetime(ContainerLifetime.Session));
        }

        return storage;
    }
}
