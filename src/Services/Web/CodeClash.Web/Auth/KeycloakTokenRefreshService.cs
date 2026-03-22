using System.Text.Json.Serialization;

namespace CodeClash.Web.Auth;

/// <summary>
/// Singleton service that exchanges a refresh token for a new access token
/// by calling the Keycloak token endpoint directly.
/// </summary>
public sealed class KeycloakTokenRefreshService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<KeycloakTokenRefreshService> logger)
{
    public async Task<TokenRefreshResult?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var options = new KeycloakWebOptions();
        configuration.GetSection(KeycloakWebOptions.SectionName).Bind(options);

        var client = httpClientFactory.CreateClient(HttpClientNames.KeycloakBackchannel);

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = options.ClientId,
            ["client_secret"] = options.ClientSecret,
            ["refresh_token"] = refreshToken
        };

        logger.LogDebug("Attempting token refresh");

        var response = await client.PostAsync(
            $"/realms/{options.Realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(form),
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Token refresh failed with status {StatusCode}: {Body}", response.StatusCode, body);
            return null;
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(ct);
        if (tokenResponse is null)
        {
            logger.LogWarning("Token response deserialization returned null");
            return null;
        }

        var expiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        logger.LogDebug("Token refresh succeeded, new expiry: {Expiry}", expiry);

        return new TokenRefreshResult(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            expiry);
    }
}

public sealed record TokenRefreshResult(string AccessToken, string RefreshToken, DateTimeOffset Expiry);

internal sealed record KeycloakTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);

public static class HttpClientNames
{
    public const string KeycloakBackchannel = "keycloak-backchannel";
}
