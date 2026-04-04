using System.Security.Claims;

namespace CodeClash.Identity.Tests.Helpers;

/// <summary>
/// Factory for building ClaimsPrincipal instances for testing.
/// </summary>
public static class ClaimsPrincipalFactory
{
    public static ClaimsPrincipal CreateAuthenticated(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreateUnauthenticated()
    {
        var identity = new ClaimsIdentity(); // no auth type = unauthenticated
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreateWithUserId(string userId)
    {
        return CreateAuthenticated(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    public static ClaimsPrincipal CreateWithSubClaim(string sub)
    {
        return CreateAuthenticated(new Claim("sub", sub));
    }

    public static ClaimsPrincipal CreateFullUser(
        string userId = "user-123",
        string username = "john.doe",
        string email = "john@example.com",
        string firstName = "John",
        string lastName = "Doe",
        params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("preferred_username", username),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.GivenName, firstName),
            new(ClaimTypes.Surname, lastName),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return CreateAuthenticated(claims.ToArray());
    }
}
