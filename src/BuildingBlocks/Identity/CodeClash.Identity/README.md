# CodeClash.Identity

Shared authentication library for CodeClash microservices using Keycloak OAuth2.0.

## Features

- JWT Bearer token authentication with Keycloak
- Easy integration with extension methods
- Centralized authentication configuration
- ClaimsPrincipal extensions for accessing user information

## Installation

Add reference to your microservice project:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\..\BuildingBlocks\CodeClash.Identity\CodeClash.Identity.csproj" />
</ItemGroup>
```

## Configuration

Add Keycloak settings to your `appsettings.json`:

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "codeclash",
    "Audience": "account",
    "RequireHttpsMetadata": false,
    "ValidateAudience": true,
    "ValidateIssuer": true
  }
}
```

For production, use HTTPS:

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

## Usage

### Basic Setup in Program.cs

```csharp
using CodeClash.Identity.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Keycloak authentication
builder.Services.AddKeycloakAuthentication(builder.Configuration);

var app = builder.Build();

// IMPORTANT: Must be called before UseAuthorization()
app.UseAuthentication();
app.UseAuthorization();

// Your endpoints here
app.MapGet("/api/secure", () => "Secure data")
    .RequireAuthorization();

app.Run();
```

## Accessing User Information

Use the ClaimsPrincipal extension methods:

```csharp
using CodeClash.Identity.Extensions;

app.MapGet("/api/profile", (ClaimsPrincipal user) =>
{
    return new
    {
        UserId = user.GetUserId(),
        Username = user.GetUsername(),
        Email = user.GetEmail(),
        FullName = user.GetFullName(),
        Roles = user.GetRoles()
    };
})
.RequireAuthorization();
```

## Authorization

Use standard ASP.NET Core authorization:

```csharp
// Require authentication
app.MapGet("/api/data", () => "Data")
    .RequireAuthorization();

// Require specific role
app.MapGet("/api/admin", () => "Admin data")
    .RequireAuthorization(policy => policy.RequireRole("admin"));

// Require specific claim
app.MapGet("/api/custom", () => "Custom data")
    .RequireAuthorization(policy =>
        policy.RequireClaim("custom_claim", "value"));
```

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `Authority` | Keycloak server URL | Required |
| `Realm` | Keycloak realm name | Required |
| `Audience` | Expected audience in JWT | Required |
| `RequireHttpsMetadata` | Require HTTPS for metadata | `true` |
| `ValidateAudience` | Validate JWT audience | `true` |
| `ValidateIssuer` | Validate JWT issuer | `true` |
| `MetadataAddress` | Custom metadata URL | Auto-generated |

## Keycloak Setup

See [KEYCLOAK_SETUP.md](./KEYCLOAK_SETUP.md) for detailed instructions on setting up Keycloak.

## Automatic Claim Population

Automatically populate command properties from claims:

```csharp
using CodeClash.Identity.Attributes;
using CodeClash.Identity.Services;

public class CreateCourseCommand
{
    [FromUserId]  // Automatically set from JWT
    public required string InstructorId { get; init; }

    public required string Title { get; init; }
}

// In your endpoint/handler
app.MapPost("/api/courses",
    [Authorize]
    async (CreateCourseCommand command,
           IClaimPopulationService claimService,
           ClaimsPrincipal user) =>
    {
        claimService.PopulateFromClaims(command, user);
        // command.InstructorId is now set!
        return Results.Ok();
    });
```

See [CLAIMS_SIMPLE.md](./CLAIMS_SIMPLE.md) for the complete guide.

## Additional Examples

See [USAGE_EXAMPLES.md](./USAGE_EXAMPLES.md) for more advanced usage patterns.
