namespace CodeClash.Identity;

public class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    public string Realm { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
