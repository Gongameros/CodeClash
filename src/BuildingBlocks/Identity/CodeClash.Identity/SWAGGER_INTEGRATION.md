# Swagger/OpenAPI Integration with Keycloak

This guide shows how to add Swagger UI with Keycloak OAuth2 authentication to your microservices.

## Installation

Add Swashbuckle package to your microservice:

```xml
<ItemGroup>
  <PackageReference Include="Swashbuckle.AspNetCore" />
</ItemGroup>
```

Update `Directory.Packages.props`:

```xml
<PackageVersion Include="Swashbuckle.AspNetCore" Version="9.0.0" />
```

## Configuration in Program.cs

```csharp
using CodeClash.Identity;
using CodeClash.Identity.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Keycloak authentication
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// Configure Keycloak options for Swagger
var keycloakOptions = new KeycloakOptions();
builder.Configuration.GetSection(KeycloakOptions.SectionName).Bind(keycloakOptions);

// Add Swagger with OAuth2
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(keycloakOptions.GetAuthorizationUrl()),
                TokenUrl = new Uri(keycloakOptions.GetTokenUrl()),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID" },
                    { "profile", "Profile" }
                }
            },
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(keycloakOptions.GetAuthorizationUrl()),
                TokenUrl = new Uri(keycloakOptions.GetTokenUrl()),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID" },
                    { "profile", "Profile" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        options.OAuthClientId("swagger-ui");
        options.OAuthAppName("Swagger UI");
        options.OAuthUsePkce();
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

## Keycloak Client Setup for Swagger

1. Go to Keycloak Admin Console
2. Select your realm (e.g., "codeclash")
3. Go to "Clients" → "Create client"
4. Configure:
   - Client ID: `swagger-ui`
   - Client type: `OpenID Connect`
   - Client authentication: `OFF` (public client)
5. Click "Next"
6. Enable:
   - Standard flow: `ON`
   - Implicit flow: `ON`
   - Direct access grants: `ON`
7. Click "Next"
8. Add Valid Redirect URIs:
   - `http://localhost:5000/swagger/oauth2-redirect.html`
   - `http://localhost:5001/swagger/oauth2-redirect.html`
   - `http://localhost:*/swagger/oauth2-redirect.html` (for development)
9. Web Origins:
   - `*` (for development)
   - Or specific origins for production (e.g., `http://localhost:5000`)
10. Click "Save"

## Using Swagger UI

1. Start your microservice
2. Navigate to `http://localhost:{port}/swagger`
3. Click the "Authorize" button (lock icon)
4. Select the scopes you need (openid, profile)
5. Click "Authorize"
6. You'll be redirected to Keycloak login
7. Enter your credentials (e.g., testuser/password)
8. You'll be redirected back to Swagger
9. Now you can test protected endpoints

## Testing Protected Endpoints

After authorizing in Swagger:

1. Find a protected endpoint
2. Click "Try it out"
3. Fill in parameters
4. Click "Execute"
5. The request will include the Bearer token automatically

## Production Configuration

For production:

1. Use HTTPS for all URLs
2. Configure specific redirect URIs (no wildcards)
3. Set Web Origins to specific domains
4. Consider using confidential client type with client secrets
5. Disable implicit flow if not needed

Example production appsettings:

```json
{
  "Keycloak": {
    "Authority": "https://keycloak.yourdomain.com",
    "Realm": "codeclash",
    "Audience": "account",
    "RequireHttpsMetadata": true,
    "ValidateAudience": true,
    "ValidateIssuer": true
  }
}
```

## Troubleshooting

### CORS Errors

- Ensure Web Origins is configured in Keycloak client
- Add `*` for development or specific origins for production

### Redirect URI Mismatch

- Check that the redirect URI in Keycloak client matches exactly
- Format: `{base_url}/swagger/oauth2-redirect.html`

### Token Not Sent

- Make sure you clicked "Authorize" and completed the OAuth flow
- Check browser console for errors
- Verify the token is in the Authorization header

### 401 Unauthorized

- Verify Keycloak is running and accessible
- Check that the realm name matches in appsettings
- Ensure the user has proper permissions
