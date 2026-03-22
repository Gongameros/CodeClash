using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CodeClash.Web.Auth;

public sealed class TokenAccessor(
    IHttpContextAccessor httpContextAccessor,
    KeycloakTokenRefreshService refreshService,
    ILogger<TokenAccessor> logger)
{
    private string? _accessToken;
    private string? _refreshToken;
    private DateTimeOffset _expiresAt;
    private bool _initialized;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null) return;
        var authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authResult.Succeeded) return;
        var props = authResult.Properties!;
        _accessToken = props.GetTokenValue("access_token");
        _refreshToken = props.GetTokenValue("refresh_token");
        var expiresAt = props.GetTokenValue("expires_at");
        if (expiresAt is not null && DateTimeOffset.TryParse(expiresAt, out var expiry))
            _expiresAt = expiry;
        _initialized = true;
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (!_initialized) await InitializeAsync();
        if (_accessToken is null) return null;
        if (!IsExpiredOrExpiringSoon) return _accessToken;
        await _lock.WaitAsync(ct);
        try
        {
            if (!IsExpiredOrExpiringSoon) return _accessToken;
            if (_refreshToken is null) return _accessToken;
            logger.LogDebug("Token expired, refreshing...");
            var refreshed = await refreshService.RefreshAsync(_refreshToken, ct);
            if (refreshed is null) { logger.LogWarning("Token refresh failed"); return _accessToken; }
            _accessToken = refreshed.AccessToken;
            _refreshToken = refreshed.RefreshToken;
            _expiresAt = refreshed.Expiry;
            logger.LogDebug("Token refreshed, new expiry: {Expiry}", _expiresAt);
            await PersistToCookieAsync(refreshed);
            return _accessToken;
        }
        finally { _lock.Release(); }
    }

    private bool IsExpiredOrExpiringSoon => _expiresAt <= DateTimeOffset.UtcNow.AddSeconds(30);

    private async Task PersistToCookieAsync(TokenRefreshResult refreshed)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null) return;
        try
        {
            var authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!authResult.Succeeded) return;
            var props = authResult.Properties!;
            props.UpdateTokenValue("access_token", refreshed.AccessToken);
            props.UpdateTokenValue("refresh_token", refreshed.RefreshToken);
            props.UpdateTokenValue("expires_at", refreshed.Expiry.ToString("o"));
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, authResult.Principal!, props);
        }
        catch (Exception ex) { logger.LogDebug(ex, "Could not persist refreshed tokens to cookie"); }
    }
}
