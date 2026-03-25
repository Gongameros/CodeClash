namespace CodeClash.E2E.Tests.Auth;

public record TestUser(string Username, string Password, string Email, string FirstName, string LastName);

public static class TestUsers
{
    public static readonly TestUser Default = new(
        "e2e-user",
        "Test1234!",
        "e2e@codeclash.test",
        "E2E",
        "TestUser");

    public static readonly TestUser Secondary = new(
        "e2e-user-2",
        "Test1234!",
        "e2e2@codeclash.test",
        "E2E",
        "SecondUser");
}
