using System.Security.Claims;
using CodeClash.Identity.Extensions;
using CodeClash.Identity.Tests.Helpers;
using Shouldly;

namespace CodeClash.Identity.Tests.Tests.Extensions;

public sealed class ClaimsPrincipalExtensionsTests
{
    // --- GetUserId ---

    [Fact]
    public void GetUserId_WithNameIdentifier_ReturnsUserId()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.NameIdentifier, "user-42"));

        principal.GetUserId().ShouldBe("user-42");
    }

    [Fact]
    public void GetUserId_WithSubClaim_ReturnsUserId()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("sub", "sub-99"));

        principal.GetUserId().ShouldBe("sub-99");
    }

    [Fact]
    public void GetUserId_PrefersNameIdentifierOverSub()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.NameIdentifier, "primary"),
            new Claim("sub", "fallback"));

        principal.GetUserId().ShouldBe("primary");
    }

    [Fact]
    public void GetUserId_NoClaims_ReturnsNull()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated();

        principal.GetUserId().ShouldBeNull();
    }

    // --- GetUsername ---

    [Fact]
    public void GetUsername_WithPreferredUsername_ReturnsUsername()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("preferred_username", "johndoe"));

        principal.GetUsername().ShouldBe("johndoe");
    }

    [Fact]
    public void GetUsername_WithNameClaim_ReturnsFallback()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.Name, "jane"));

        principal.GetUsername().ShouldBe("jane");
    }

    [Fact]
    public void GetUsername_PrefersPreferredUsernameOverName()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("preferred_username", "primary"),
            new Claim(ClaimTypes.Name, "fallback"));

        principal.GetUsername().ShouldBe("primary");
    }

    [Fact]
    public void GetUsername_NoClaims_ReturnsNull()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated();

        principal.GetUsername().ShouldBeNull();
    }

    // --- GetEmail ---

    [Fact]
    public void GetEmail_WithEmailClaim_ReturnsEmail()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.Email, "john@example.com"));

        principal.GetEmail().ShouldBe("john@example.com");
    }

    [Fact]
    public void GetEmail_WithShortEmailClaim_ReturnsFallback()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("email", "jane@example.com"));

        principal.GetEmail().ShouldBe("jane@example.com");
    }

    [Fact]
    public void GetEmail_NoClaims_ReturnsNull()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated();

        principal.GetEmail().ShouldBeNull();
    }

    // --- GetRoles ---

    [Fact]
    public void GetRoles_WithMultipleRoles_ReturnsAll()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "editor"),
            new Claim(ClaimTypes.Role, "viewer"));

        var roles = principal.GetRoles().ToList();

        roles.Count.ShouldBe(3);
        roles.ShouldContain("admin");
        roles.ShouldContain("editor");
        roles.ShouldContain("viewer");
    }

    [Fact]
    public void GetRoles_NoRoles_ReturnsEmpty()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated();

        principal.GetRoles().ShouldBeEmpty();
    }

    // --- HasRole ---

    [Fact]
    public void HasRole_UserHasRole_ReturnsTrue()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.Role, "admin"));

        principal.HasRole("admin").ShouldBeTrue();
    }

    [Fact]
    public void HasRole_UserDoesNotHaveRole_ReturnsFalse()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.Role, "editor"));

        principal.HasRole("admin").ShouldBeFalse();
    }

    // --- GetFirstName ---

    [Fact]
    public void GetFirstName_WithGivenNameClaim_ReturnsFirstName()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.GivenName, "John"));

        principal.GetFirstName().ShouldBe("John");
    }

    [Fact]
    public void GetFirstName_WithGivenNameShortClaim_ReturnsFallback()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("given_name", "Jane"));

        principal.GetFirstName().ShouldBe("Jane");
    }

    [Fact]
    public void GetFirstName_NoClaims_ReturnsNull()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated();

        principal.GetFirstName().ShouldBeNull();
    }

    // --- GetLastName ---

    [Fact]
    public void GetLastName_WithSurnameClaim_ReturnsLastName()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.Surname, "Doe"));

        principal.GetLastName().ShouldBe("Doe");
    }

    [Fact]
    public void GetLastName_WithFamilyNameClaim_ReturnsFallback()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("family_name", "Smith"));

        principal.GetLastName().ShouldBe("Smith");
    }

    [Fact]
    public void GetLastName_NoClaims_ReturnsNull()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated();

        principal.GetLastName().ShouldBeNull();
    }

    // --- GetFullName ---

    [Fact]
    public void GetFullName_WithFirstAndLastName_ReturnsCombined()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"));

        principal.GetFullName().ShouldBe("John Doe");
    }

    [Fact]
    public void GetFullName_OnlyFirstName_FallsBackToNameClaim()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim("name", "John D."));

        // lastName is null, so it falls back to "name" claim
        principal.GetFullName().ShouldBe("John D.");
    }

    [Fact]
    public void GetFullName_OnlyLastName_FallsBackToNameClaim()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim("name", "J. Doe"));

        principal.GetFullName().ShouldBe("J. Doe");
    }

    [Fact]
    public void GetFullName_NoNames_FallsBackToNameClaim()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("name", "Full Name Here"));

        principal.GetFullName().ShouldBe("Full Name Here");
    }

    [Fact]
    public void GetFullName_NoClaims_ReturnsNull()
    {
        var principal = ClaimsPrincipalFactory.CreateAuthenticated();

        principal.GetFullName().ShouldBeNull();
    }

    // --- Full user scenario ---

    [Fact]
    public void FullUser_AllClaimsPresent_AllMethodsReturnCorrectValues()
    {
        var principal = ClaimsPrincipalFactory.CreateFullUser(
            userId: "uid-1",
            username: "jdoe",
            email: "jdoe@test.com",
            firstName: "John",
            lastName: "Doe",
            roles: ["admin", "user"]);

        principal.GetUserId().ShouldBe("uid-1");
        principal.GetUsername().ShouldBe("jdoe");
        principal.GetEmail().ShouldBe("jdoe@test.com");
        principal.GetFirstName().ShouldBe("John");
        principal.GetLastName().ShouldBe("Doe");
        principal.GetFullName().ShouldBe("John Doe");
        principal.GetRoles().Count().ShouldBe(2);
    }
}
