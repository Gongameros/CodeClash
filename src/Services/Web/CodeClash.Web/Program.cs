using CodeClash.Identity.Extensions;
using CodeClash.ServiceDefaults;
using CodeClash.Shared.Constants;
using CodeClash.Web.ApiClients;
using CodeClash.Web.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddKeycloakWebAuthentication(Resources.Keycloak);
builder.Services.AddCascadingAuthenticationState();

// ── Token forwarding ──────────────────────────────────────────────────────────
builder.Services.AddScoped<TokenAccessor>();
builder.Services.AddSingleton<KeycloakTokenRefreshService>();

// Backchannel client used by KeycloakTokenRefreshService for token refresh calls
builder.Services.AddHttpClient(HttpClientNames.KeycloakBackchannel, client =>
{
    // Use the fixed HTTP port so the backchannel refresh URL is stable across Aspire restarts.
    // KC_HOSTNAME on the container forces this same URL as the token issuer.
    var keycloakBaseUrl = builder.Configuration["CODECLASH_KEYCLOAK_HTTP"]
        ?? builder.Configuration["CODECLASH_KEYCLOAK_HTTPS"];
    client.BaseAddress = new Uri(keycloakBaseUrl!);
});

// ── Courses API client ────────────────────────────────────────────────────────
builder.Services.AddHttpClient<ICoursesApiClient, CoursesApiClient>(client =>
    {
        client.BaseAddress = new Uri($"https+http://{Resources.CoursesService}");
    })
    .AddStandardResilienceHandler();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Login: challenge the OIDC scheme → Keycloak redirect
app.MapGet("/auth/login", (string? returnUrl) =>
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
        [OpenIdConnectDefaults.AuthenticationScheme]));

// Logout: clear cookie + notify Keycloak
app.MapGet("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/" });
});

app.MapDefaultEndpoints();

app.MapRazorComponents<CodeClash.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
