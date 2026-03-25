using Aspire.Hosting;
using Aspire.Hosting.Testing;
using CodeClash.Shared.Constants;

namespace CodeClash.E2E.Tests.Infrastructure;

public class AspireFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public DistributedApplication App => _app ?? throw new InvalidOperationException("Aspire app not started");
    public string WebBaseUrl { get; private set; } = string.Empty;
    public string KeycloakBaseUrl { get; private set; } = string.Empty;
    public string KeycloakAdminPassword { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.CodeClash_AppHost>();

        // Read the keycloak password from configuration (loaded from user secrets)
        KeycloakAdminPassword = appHost.Configuration["Parameters:keycloak-password"] ?? "admin";

        // Ensure internal-api-key is set for tests
        if (string.IsNullOrEmpty(appHost.Configuration["Parameters:internal-api-key"]))
            appHost.Configuration["Parameters:internal-api-key"] = "e2e-test-api-key";

        _app = await appHost.BuildAsync();
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await _app.StartAsync(cts.Token);

        // Extract endpoints
        WebBaseUrl = _app.GetEndpoint(Resources.WebService, "https")?.ToString()
                     ?? _app.GetEndpoint(Resources.WebService, "http")!.ToString();
        WebBaseUrl = WebBaseUrl.TrimEnd('/');

        KeycloakBaseUrl = _app.GetEndpoint(Resources.Keycloak, "http")!.ToString();
        KeycloakBaseUrl = KeycloakBaseUrl.TrimEnd('/');
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
