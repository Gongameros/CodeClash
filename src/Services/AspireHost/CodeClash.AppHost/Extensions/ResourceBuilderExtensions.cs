using System.Diagnostics;

namespace CodeClash.AppHost.Extensions;

internal static class ResourceBuilderExtensions
{
    extension<T>(IResourceBuilder<T> builder) where T : IResourceWithEndpoints
    {
        internal IResourceBuilder<T> WithScalar()
        {
            return builder.WithOpenApiDocs("scalar-docs", "Scalar API Documentation", "scalar/v1");
        }

        private IResourceBuilder<T> WithOpenApiDocs(string name, string displayName, string openApiUiPath)
        {
            return builder.WithCommand(name,
                displayName,
                _ =>
                {
                    try
                    {
                        // Base URL
                        var endpoint = builder.GetEndpoint("http");

                        var url = $"{endpoint.Url}/{openApiUiPath}";

                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

                        return Task.FromResult(new ExecuteCommandResult { Success = true });
                    }
                    catch (Exception e)
                    {
                        return Task.FromResult(new ExecuteCommandResult { Success = false, ErrorMessage = e.ToString() });
                    }
                },
                new CommandOptions
                {
                    IconName = "Document",
                    IconVariant = IconVariant.Filled
                });
        }
    }
}
