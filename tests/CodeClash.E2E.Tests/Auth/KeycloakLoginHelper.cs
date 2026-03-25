namespace CodeClash.E2E.Tests.Auth;

public static class KeycloakLoginHelper
{
    public static async Task LoginAsync(IPage page, string webBaseUrl, TestUser user)
    {
        // Navigate to login endpoint which triggers OIDC challenge -> Keycloak redirect
        await page.GotoAsync($"{webBaseUrl}/auth/login",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Wait for Keycloak login form
        await page.WaitForSelectorAsync("#username", new PageWaitForSelectorOptions
        {
            Timeout = 30_000
        });

        // Fill credentials
        await page.FillAsync("#username", user.Username);
        await page.FillAsync("#password", user.Password);

        // Submit
        await page.ClickAsync("#kc-login");

        // Wait for redirect back to the app
        await page.WaitForURLAsync($"{webBaseUrl}/**", new PageWaitForURLOptions
        {
            Timeout = 30_000
        });

        // Wait for Blazor to establish SignalR connection
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public static async Task LogoutAsync(IPage page, string webBaseUrl)
    {
        await page.GotoAsync($"{webBaseUrl}/auth/logout",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }
}
