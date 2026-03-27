using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeClash.E2E.Tests.Auth;

public class KeycloakAdminClient
{
    private const string Realm = "codeclash";

    private readonly HttpClient _http;
    private readonly string _adminPassword;
    private readonly string _webBaseUrl;

    public KeycloakAdminClient(string keycloakBaseUrl, string adminPassword, string webBaseUrl)
    {
        _adminPassword = adminPassword;
        _webBaseUrl = webBaseUrl.TrimEnd('/');
        var baseUrl = keycloakBaseUrl.TrimEnd('/');
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task EnsureTestUsersAsync()
    {
        await AuthenticateAsync();
        await EnsureRealmAsync();
        await EnsureClientAsync();
        await EnsureUserAsync(TestUsers.Default);
        await EnsureUserAsync(TestUsers.Secondary);
    }

    private async Task EnsureRealmAsync()
    {
        // Check if realm exists
        var response = await _http.GetAsync($"/admin/realms/{Realm}");
        if (response.StatusCode == HttpStatusCode.OK)
            return;

        // Create realm
        var realmPayload = new
        {
            realm = Realm,
            enabled = true,
            displayName = "CodeClash",
            registrationAllowed = false,
            loginWithEmailAllowed = true,
            duplicateEmailsAllowed = false,
            sslRequired = "none",
            // Token settings
            accessTokenLifespan = 300,
            ssoSessionIdleTimeout = 1800,
            ssoSessionMaxLifespan = 36000,
            offlineSessionIdleTimeout = 2592000,
            refreshTokenMaxReuse = 0,
        };

        var createResponse = await _http.PostAsJsonAsync("/admin/realms", realmPayload);
        if (createResponse.StatusCode == HttpStatusCode.Conflict)
            return; // Created concurrently

        createResponse.EnsureSuccessStatusCode();
    }

    private async Task EnsureClientAsync()
    {
        const string clientId = "codeclash-web";

        // Check if client exists
        var clients = await _http.GetFromJsonAsync<JsonElement[]>(
            $"/admin/realms/{Realm}/clients?clientId={clientId}");

        if (clients is { Length: > 0 })
        {
            // Update redirect URIs in case the web URL changed
            var existingId = clients[0].GetProperty("id").GetString()!;
            await UpdateClientRedirectUrisAsync(existingId);
            return;
        }

        // Create public OIDC client
        var clientPayload = new
        {
            clientId,
            name = "CodeClash Web",
            protocol = "openid-connect",
            publicClient = true,
            standardFlowEnabled = true,
            directAccessGrantsEnabled = true,
            enabled = true,
            redirectUris = new[] { $"{_webBaseUrl}/*", "http://localhost:*/*" },
            webOrigins = new[] { "*" },
            frontchannelLogout = true,
            attributes = new Dictionary<string, string>
            {
                ["post.logout.redirect.uris"] = $"{_webBaseUrl}/*"
            }
        };

        var createResponse = await _http.PostAsJsonAsync($"/admin/realms/{Realm}/clients", clientPayload);
        if (createResponse.StatusCode == HttpStatusCode.Conflict)
            return;

        createResponse.EnsureSuccessStatusCode();
    }

    private async Task UpdateClientRedirectUrisAsync(string internalId)
    {
        var update = new
        {
            publicClient = true,
            directAccessGrantsEnabled = true,
            redirectUris = new[] { $"{_webBaseUrl}/*", "http://localhost:*/*" },
            webOrigins = new[] { "*" },
            attributes = new Dictionary<string, string>
            {
                ["post.logout.redirect.uris"] = $"{_webBaseUrl}/*"
            }
        };

        await _http.PutAsJsonAsync($"/admin/realms/{Realm}/clients/{internalId}", update);
    }

    private async Task EnsureUserAsync(TestUser user)
    {
        // Check if user exists
        var users = await _http.GetFromJsonAsync<JsonElement[]>(
            $"/admin/realms/{Realm}/users?username={user.Username}&exact=true");

        if (users is { Length: > 0 })
            return;

        // Create user
        var userPayload = new
        {
            username = user.Username,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            enabled = true,
            emailVerified = true,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = user.Password,
                    temporary = false
                }
            }
        };

        var response = await _http.PostAsJsonAsync($"/admin/realms/{Realm}/users", userPayload);

        // 409 Conflict means user already exists (race condition) - that's fine
        if (response.StatusCode == HttpStatusCode.Conflict)
            return;

        response.EnsureSuccessStatusCode();
    }

    private async Task AuthenticateAsync()
    {
        const int maxRetries = 10;
        HttpResponseMessage? lastResponse = null;

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            if (attempt > 0)
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(2 * attempt, 10)));

            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = "admin",
                ["password"] = _adminPassword
            });

            lastResponse = await _http.PostAsync(
                "/realms/master/protocol/openid-connect/token", tokenRequest);

            if (lastResponse.IsSuccessStatusCode)
            {
                var token = await lastResponse.Content.ReadFromJsonAsync<TokenResponse>();
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token!.AccessToken);
                return;
            }
        }

        var body = lastResponse is not null ? await lastResponse.Content.ReadAsStringAsync() : "no response";
        throw new HttpRequestException(
            $"Keycloak admin auth failed after {maxRetries} attempts. " +
            $"Status: {lastResponse?.StatusCode}, Body: {body}");
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }
}
