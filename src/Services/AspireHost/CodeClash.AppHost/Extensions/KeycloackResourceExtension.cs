using CodeClash.Shared.Constants;

namespace CodeClash.AppHost.Extensions;

public static class KeycloakResourceExtension
{
    private const int KeycloakPort = 8079;
    private const string KeycloakVolumeName = "codeclash-keycloak-data";

    public static IResourceBuilder<KeycloakResource> AddKeycloakResource(
        this IDistributedApplicationBuilder builder)
    {
        var keycloakUsername = builder.AddParameter("keycloak-username", "admin");
        var keycloakPassword = builder.AddParameter("keycloak-password", secret: true);

        return builder.AddKeycloak(
                name: Resources.Keycloak,
                port: KeycloakPort,
                adminUsername: keycloakUsername,
                adminPassword: keycloakPassword)
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume(KeycloakVolumeName)
            .WithOtlpExporter();
    }
}
