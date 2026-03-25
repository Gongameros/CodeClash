namespace CodeClash.E2E.Tests.Infrastructure;

[Collection(E2ECollection.Name)]
[Trait("Category", "E2E")]
public abstract class UnauthenticatedE2ETest : IAsyncLifetime
{
    protected readonly E2EFixture Fixture;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;

    protected UnauthenticatedE2ETest(E2EFixture fixture) => Fixture = fixture;

    public virtual async ValueTask InitializeAsync()
    {
        Context = await Fixture.CreateContextAsync();
        Page = await Context.NewPageAsync();
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
}
