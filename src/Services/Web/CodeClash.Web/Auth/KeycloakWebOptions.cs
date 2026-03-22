namespace CodeClash.Web.Auth;

public sealed class KeycloakWebOptions
{
    public const string SectionName = "Keycloak";

    public string Realm { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
