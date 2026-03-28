using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using CodeClash.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeClash.E2E.Tests.Infrastructure;

public class AspireFixture : IAsyncLifetime
{
    private DistributedApplication? _app;

    public DistributedApplication App => _app ?? throw new InvalidOperationException("Aspire app not started");
    public string WebBaseUrl { get; private set; } = string.Empty;
    public const string InternalApiKey = "e2e-test-api-key";
    public const string KeycloakAdminPassword = "admin";
    public string KeycloakBaseUrl { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.CodeClash_AppHost>(["--Testing:IsE2E=true",
                $"--Parameters:keycloak-password={KeycloakAdminPassword}",
                $"--Parameters:internal-api-key={InternalApiKey}"]);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter("Aspire", LogLevel.Debug);
        });

        _app = await appHost.BuildAsync();
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        InjectCodersEnvironmentVariables(appHost);
        InjectCoursesEnvironmentVariables(appHost);
        InjectGatewayEnvironmentVariables(appHost);
        InjectWebEnvironmentVariables(appHost);
        InjectKestrelCertEnvironmentVariables(appHost);

        var notifications = _app.Services.GetRequiredService<ResourceNotificationService>();

        // Log all resource state changes in the background for diagnostics
        _ = Task.Run(async () =>
        {
            try
            {
                await foreach (var evt in notifications.WatchAsync(cts.Token))
                {
                    Console.Error.WriteLine(
                        $"[ASPIRE] Resource '{evt.Resource.Name}' -> State: {evt.Snapshot.State?.Text ?? "null"}, " +
                        $"Health: {evt.Snapshot.HealthStatus?.ToString() ?? "null"}");
                }
            }
            catch (OperationCanceledException) { }
        }, cts.Token);

        // Capture resource logs for all project resources to diagnose startup failures
        var loggerService = _app.Services.GetRequiredService<ResourceLoggerService>();
        var resourcesToDiagnose = new[] { Resources.CodersService, Resources.CoursesService, Resources.WebService, Resources.GatewayService };
        foreach (var resourceName in resourcesToDiagnose)
        {
            var name = resourceName;
            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var batch in loggerService.WatchAsync(name).WithCancellation(cts.Token))
                    {
                        foreach (var log in batch)
                            Console.Error.WriteLine($"[LOG:{name}] {log.Content}");
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { Console.Error.WriteLine($"[LOG:{name}] Watch error: {ex.Message}"); }
            }, cts.Token);
        }

        await _app.StartAsync(cts.Token);

        // Wait for Keycloak and Web to be healthy before proceeding
        try
        {
            await notifications.WaitForResourceHealthyAsync(Resources.Keycloak, cts.Token);
            await notifications.WaitForResourceAsync(Resources.WebService, KnownResourceStates.Running, cts.Token);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ASPIRE] Resource wait failed: {ex.Message}");

            // Give time for pending resource logs to flush
            await Task.Delay(5000);

            throw;
        }

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

    private void InjectCodersEnvironmentVariables(IDistributedApplicationTestingBuilder appHost)
    {
        var env = GetCommonApiEnvironmentVariables();
        InjectEnvironmentVariables(appHost, Resources.CodersService, env);
    }

    private void InjectCoursesEnvironmentVariables(IDistributedApplicationTestingBuilder appHost)
    {
        var env = GetCommonApiEnvironmentVariables();
        InjectEnvironmentVariables(appHost, Resources.CoursesService, env);
    }

    private void InjectGatewayEnvironmentVariables(IDistributedApplicationTestingBuilder appHost)
    {
        var env = GetCommonApiEnvironmentVariables();
        InjectEnvironmentVariables(appHost, Resources.GatewayService, env);
    }

    private void InjectWebEnvironmentVariables(IDistributedApplicationTestingBuilder appHost)
    {
        const string keycloakClientId = "codeclash-web";
        var env = new Dictionary<string, string>
        {
            // Keycloak
            ["Keycloak:Realm"] = CommonConstants.KeycloakRealm,
            ["Keycloak:ClientId"] = keycloakClientId,
            ["Keycloak:ClientSecret"] = CommonConstants.KeycloakClientTestSecret,
        };

        InjectEnvironmentVariables(appHost, Resources.WebService, env);
    }

    private Dictionary<string, string> GetCommonApiEnvironmentVariables()
    {
        const string keycloakClientId = "codeclash-api";
        return new Dictionary<string, string>
        {
            // Keycloak
            ["Keycloak:Realm"] = CommonConstants.KeycloakRealm,
            ["Keycloak:Audience"] = keycloakClientId,
            ["Keycloak:BaseUrl"] = "http://localhost:8079"
        };
    }

    private void InjectKestrelCertEnvironmentVariables(IDistributedApplicationTestingBuilder appHost)
    {
        var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
        var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

        if (string.IsNullOrEmpty(certPath)) return;

        var certEnv = new Dictionary<string, string>
        {
            ["ASPNETCORE_Kestrel__Certificates__Default__Path"] = certPath,
            ["ASPNETCORE_Kestrel__Certificates__Default__Password"] = certPassword ?? ""
        };

        foreach (var resource in appHost.Resources.OfType<ProjectResource>())
        {
            resource.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
            {
                foreach (var kvp in certEnv)
                    context.EnvironmentVariables[kvp.Key] = kvp.Value;
            }));
        }

        Console.Error.WriteLine($"[ASPIRE] Injected Kestrel cert path into all project resources: {certPath}");
    }

    private void InjectEnvironmentVariables(
        IDistributedApplicationTestingBuilder appHost,
        string resourceName,
        IDictionary<string, string> variables)
    {
        ProjectResource resource = appHost.Resources
            .OfType<ProjectResource>()
            .Single(r => r.Name == resourceName);

        resource.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            foreach (var kvp in variables)
            {
                context.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
        }));
    }
}
