using CodeClash.Shared.Constants;

namespace CodeClash.AppHost.Extensions;

public static class KeycloakResourceExtension
{
    private const int KeycloakPort = 8079;
    private const string KeycloakVolumeName = "codeclash-keycloak-data";

    public static IResourceBuilder<KeycloakResource> AddKeycloakResource(
        this IDistributedApplicationBuilder builder)
    {
        var isE2E = builder.Configuration["Testing:IsE2E"] == "true";

        var keycloakUsername = builder.AddParameter("keycloak-username", "admin");
        var keycloakPassword = builder.AddParameter("keycloak-password", secret: true);

        // Use a random port for tests to avoid conflicts with the dev container
        var keycloak = builder.AddKeycloak(
                name: Resources.Keycloak,
                port: isE2E ? null : KeycloakPort,
                adminUsername: keycloakUsername,
                adminPassword: keycloakPassword)
            .WithRealmImport("KeycloakRealms")
            .WithEndpoint("http", endpoint =>
            {
                endpoint.IsExternal = true;
                endpoint.Port = null;
                endpoint.TargetPort = 8080;
            })
            .WithOtlpExporter();

        if (!isE2E)
        {
            keycloak.WithLifetime(ContainerLifetime.Persistent)
                .WithDataVolume(KeycloakVolumeName);
        }

        return keycloak;
    }
}
