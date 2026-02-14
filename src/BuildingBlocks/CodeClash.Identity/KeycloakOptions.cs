namespace CodeClash.Identity;

public class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public string MetadataAddress { get; set; } = string.Empty;

    public string GetMetadataAddress()
    {
        if (!string.IsNullOrEmpty(MetadataAddress))
        {
            return MetadataAddress;
        }

        return $"{Authority}/realms/{Realm}/.well-known/openid-configuration";
    }

    public string GetAuthorizationUrl()
    {
        return $"{Authority}/realms/{Realm}/protocol/openid-connect/auth";
    }

    public string GetTokenUrl()
    {
        return $"{Authority}/realms/{Realm}/protocol/openid-connect/token";
    }
}
