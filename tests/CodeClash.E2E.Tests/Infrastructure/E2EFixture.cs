using CodeClash.E2E.Tests.Auth;

namespace CodeClash.E2E.Tests.Infrastructure;

public class E2EFixture : IAsyncLifetime
{
    private readonly AspireFixture _aspire = new();
    private readonly PlaywrightFixture _playwright = new();

    public string WebBaseUrl => _aspire.WebBaseUrl;
    public string KeycloakBaseUrl => _aspire.KeycloakBaseUrl;
    public IBrowser Browser => _playwright.Browser;

    public async ValueTask InitializeAsync()
    {
        await _aspire.InitializeAsync();
        await _playwright.InitializeAsync();

        // Ensure test users exist in Keycloak (realm + clients already exist from dev setup)
        var keycloakAdmin = new KeycloakAdminClient(
            _aspire.KeycloakBaseUrl, AspireFixture.KeycloakAdminPassword, _aspire.WebBaseUrl);
        await keycloakAdmin.EnsureTestUsersAsync();
    }

    public async Task<IBrowserContext> CreateContextAsync()
    {
        return await _playwright.CreateContextAsync();
    }

    public async Task<IBrowserContext> CreateAuthenticatedContextAsync(TestUser user)
    {
        var context = await _playwright.CreateContextAsync();
        var page = await context.NewPageAsync();
        await KeycloakLoginHelper.LoginAsync(page, WebBaseUrl, user);
        return context;
    }

    public async ValueTask DisposeAsync()
    {
        await _playwright.DisposeAsync();
        await _aspire.DisposeAsync();
    }
}
