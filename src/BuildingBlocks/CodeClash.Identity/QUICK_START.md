# Quick Start Guide

## What is CodeClash.Identity?

CodeClash.Identity is a shared authentication library that provides Keycloak OAuth2.0/OpenID Connect integration for all CodeClash microservices. It centralizes authentication logic so you don't have to repeat the same configuration in every service.

## Quick Setup (3 Steps)

### 1. Add Project Reference

In your microservice `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\..\BuildingBlocks\CodeClash.Identity\CodeClash.Identity.csproj" />
</ItemGroup>
```

### 2. Configure Keycloak in appsettings.json

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "codeclash",
    "Audience": "account",
    "RequireHttpsMetadata": false
  }
}
```

### 3. Update Program.cs

```csharp
using CodeClash.Identity.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Keycloak authentication
builder.Services.AddKeycloakAuthentication(builder.Configuration);

var app = builder.Build();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Protected endpoint
app.MapGet("/api/data", () => "Protected data")
    .RequireAuthorization();

app.Run();
```

## Testing

### Get a Token from Keycloak

```bash
curl -X POST 'http://localhost:8080/realms/codeclash/protocol/openid-connect/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d 'client_id=swagger-ui' \
  -d 'username=testuser' \
  -d 'password=password' \
  -d 'grant_type=password'
```

### Use the Token

```bash
curl -X GET 'http://localhost:5000/api/data' \
  -H 'Authorization: Bearer YOUR_ACCESS_TOKEN'
```

## Features Included

- JWT Bearer token validation
- Automatic token validation against Keycloak
- ClaimsPrincipal extension methods for easy user data access
- Support for role-based and claims-based authorization
- Environment-specific configuration (dev/prod)

## Current Integrations

The library is already integrated into:
- **CodeClash.Coders** - Coders microservice
- **CodeClash.Courses** - Courses microservice

## Next Steps

1. **Set up Keycloak** - See [KEYCLOAK_SETUP.md](./KEYCLOAK_SETUP.md)
2. **Add Swagger UI** - See [SWAGGER_INTEGRATION.md](./SWAGGER_INTEGRATION.md)
3. **Advanced Usage** - See [USAGE_EXAMPLES.md](./USAGE_EXAMPLES.md)
4. **Full Documentation** - See [README.md](./README.md)

## Project Structure

```
CodeClash.Identity/
├── KeycloakOptions.cs              # Configuration class
├── Extensions/
│   ├── AuthenticationExtensions.cs # JWT Bearer setup
│   └── ClaimsPrincipalExtensions.cs # User info helpers
├── README.md                        # Full documentation
├── QUICK_START.md                   # This file
├── KEYCLOAK_SETUP.md               # Keycloak setup guide
├── SWAGGER_INTEGRATION.md          # Swagger OAuth2 guide
└── USAGE_EXAMPLES.md               # Advanced examples
```

## Common Use Cases

### Accessing User Information

```csharp
using CodeClash.Identity.Extensions;

app.MapGet("/api/me", (ClaimsPrincipal user) => new
{
    UserId = user.GetUserId(),
    Username = user.GetUsername(),
    Email = user.GetEmail()
})
.RequireAuthorization();
```

### Role-Based Authorization

```csharp
app.MapGet("/api/admin", () => "Admin only")
    .RequireAuthorization(policy => policy.RequireRole("admin"));
```

### Multiple Roles (Any)

```csharp
app.MapGet("/api/moderator", () => "Moderator area")
    .RequireAuthorization(policy =>
        policy.RequireRole("admin", "moderator"));
```

## Support

For issues or questions:
1. Check the documentation files in this directory
2. Review the example integrations in Coders and Courses services
3. See the USAGE_EXAMPLES.md for advanced patterns
