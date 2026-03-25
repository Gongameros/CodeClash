using CodeClash.E2E.Tests.Auth;
using CodeClash.E2E.Tests.Infrastructure;
using CodeClash.E2E.Tests.PageObjects.Components;

namespace CodeClash.E2E.Tests.Tests;

[Collection(E2ECollection.Name)]
[Trait("Category", "E2E")]
public class AuthenticationFlowTests : IAsyncLifetime
{
    private readonly E2EFixture _fixture;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public AuthenticationFlowTests(E2EFixture fixture) => _fixture = fixture;

    public async ValueTask InitializeAsync()
    {
        _context = await _fixture.CreateContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async ValueTask DisposeAsync() => await _context.CloseAsync();

    [Fact]
    public async Task UnauthenticatedUser_CanSeeHomePage()
    {
        await _page.GotoAsync(_fixture.WebBaseUrl,
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var heroText = _page.GetByText("<CodeClash />");
        await heroText.WaitForAsync();
        (await heroText.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task UnauthenticatedUser_SeesLoginButton()
    {
        await _page.GotoAsync(_fixture.WebBaseUrl,
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var nav = new NavBarComponent(_page);
        (await nav.LoginButton.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task LoginButton_RedirectsToKeycloak()
    {
        await _page.GotoAsync($"{_fixture.WebBaseUrl}/auth/login",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Should be on Keycloak login page
        var usernameField = _page.Locator("#username");
        await usernameField.WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });
        (await usernameField.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task SuccessfulLogin_ShowsLogoutButton()
    {
        await KeycloakLoginHelper.LoginAsync(_page, _fixture.WebBaseUrl, TestUsers.Default);

        var nav = new NavBarComponent(_page);
        await nav.LogoutButton.WaitForAsync(new LocatorWaitForOptions { Timeout = 10_000 });
        (await nav.LogoutButton.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task ProtectedPage_RedirectsToLogin()
    {
        await _page.GotoAsync($"{_fixture.WebBaseUrl}/courses/new",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Should be redirected to Keycloak login page
        var usernameField = _page.Locator("#username");
        await usernameField.WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });
        (await usernameField.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task Logout_RedirectsToHomePage()
    {
        // Login first
        await KeycloakLoginHelper.LoginAsync(_page, _fixture.WebBaseUrl, TestUsers.Default);

        // Logout
        await KeycloakLoginHelper.LogoutAsync(_page, _fixture.WebBaseUrl);

        // Wait for redirect back to home
        await _page.WaitForURLAsync($"{_fixture.WebBaseUrl}/**",
            new PageWaitForURLOptions { Timeout = 30_000 });
    }
}
