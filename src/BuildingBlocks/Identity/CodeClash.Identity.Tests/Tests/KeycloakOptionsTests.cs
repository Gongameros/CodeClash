using Shouldly;

namespace CodeClash.Identity.Tests.Tests;

public sealed class KeycloakOptionsTests
{
    [Fact]
    public void SectionName_IsKeycloak()
    {
        KeycloakOptions.SectionName.ShouldBe("Keycloak");
    }

    [Fact]
    public void DefaultValues_AreEmptyStrings()
    {
        var options = new KeycloakOptions();

        options.Realm.ShouldBe(string.Empty);
        options.Audience.ShouldBe(string.Empty);
        options.ClientId.ShouldBe(string.Empty);
        options.ClientSecret.ShouldBe(string.Empty);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var options = new KeycloakOptions
        {
            Realm = "my-realm",
            Audience = "my-audience",
            ClientId = "my-client",
            ClientSecret = "secret-123"
        };

        options.Realm.ShouldBe("my-realm");
        options.Audience.ShouldBe("my-audience");
        options.ClientId.ShouldBe("my-client");
        options.ClientSecret.ShouldBe("secret-123");
    }
}
