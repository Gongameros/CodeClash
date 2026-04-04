using CodeClash.E2E.Tests.Auth;

namespace CodeClash.E2E.Tests.Infrastructure;

[Collection(E2ECollection.Name)]
[Trait("Category", "E2E")]
public abstract class BaseE2ETest : IAsyncLifetime
{
    protected readonly E2EFixture Fixture;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;

    protected BaseE2ETest(E2EFixture fixture) => Fixture = fixture;

    public virtual async ValueTask InitializeAsync()
    {
        Context = await Fixture.CreateAuthenticatedContextAsync(TestUsers.Default);
        Page = Context.Pages[0];
    }

    public virtual async ValueTask DisposeAsync()
    {
        await Context.CloseAsync();
    }

    protected async Task NavigateAsync(string relativeUrl)
    {
        await Page.GotoAsync($"{Fixture.WebBaseUrl}{relativeUrl}",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    protected async Task WaitForMudLoadedAsync()
    {
        // Wait for MudBlazor skeleton loaders to disappear
        await Page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.mud-skeleton').length === 0",
            null,
            new PageWaitForFunctionOptions { Timeout = 15_000 });
    }

    protected ILocator SnackbarMessage => Page.Locator(".mud-snackbar .mud-snackbar-content-message");

    protected async Task WaitForSnackbarAsync(string expectedText, int timeoutMs = 10_000)
    {
        await Page.Locator(".mud-snackbar")
            .Filter(new LocatorFilterOptions { HasText = expectedText })
            .WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs });
    }
}
