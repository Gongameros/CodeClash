# Automatic Claim Population

This feature allows you to automatically populate properties in your commands/requests from the authenticated user's claims using attributes.

## Features

- Automatically extract user information from JWT claims
- Type-safe with support for string, Guid, int, long, bool, DateTime, etc.
- Works with both Wolverine commands and ASP.NET Core controllers
- Configurable required/optional claims
- Support for alternate claim types

## Quick Start

### 1. Mark Properties with Attributes

```csharp
public class CreateCourseCommand
{
    // Automatically populated from user's ID claim
    [FromUserId]
    public required string InstructorId { get; init; }

    // Other properties from request body
    public required string Title { get; init; }
    public required string Description { get; init; }
}
```

### 2. Configure in Program.cs

#### For Wolverine (Recommended)

```csharp
using CodeClash.Identity.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Keycloak authentication
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// Add HttpContextAccessor (required for claims access)
builder.Services.AddHttpContextAccessor();

// Configure Wolverine with claim population
builder.Host.UseWolverine(opts =>
{
    opts.UseClaimPopulation();
});
```

#### For ASP.NET Core Controllers

```csharp
using CodeClash.Identity.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddClaimPopulation(); // Adds the filter
```

### 3. Use in Your Endpoints

```csharp
// Wolverine HTTP endpoint
app.MapPost("/api/courses",
    (CreateCourseCommand command) =>
    {
        // command.InstructorId is automatically set from claims!
        return Results.Ok(command);
    });

// The InstructorId will be populated automatically before the handler executes
```

## Available Attributes

### `[FromUserId]`

Automatically populates from the user's ID claim (`sub` or `NameIdentifier`).

```csharp
[FromUserId]
public required string UserId { get; init; }
```

### `[FromClaim]`

Populates from a specific claim type.

```csharp
// Using standard claim type
[FromClaim(ClaimTypes.Email)]
public required string Email { get; init; }

// Using custom claim name
[FromClaim("tenant_id")]
public required string TenantId { get; init; }

// Optional claim (won't throw if missing)
[FromClaim("department", Required = false)]
public string? Department { get; init; }

// With alternate claim type
[FromClaim(ClaimType = "org_id", AlternateClaimType = "organization_id")]
public required string OrganizationId { get; init; }
```

## Complete Example

### Command Definition

```csharp
using CodeClash.Identity.Attributes;
using System.Security.Claims;

public class CreateAppointmentCommand
{
    // Automatically from authenticated user
    [FromUserId]
    public required string ExpertId { get; init; }

    // Automatically from email claim
    [FromClaim(ClaimTypes.Email)]
    public required string ExpertEmail { get; init; }

    // Optional claim
    [FromClaim("timezone", Required = false)]
    public string? Timezone { get; init; }

    // From request body/query
    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }
    public required string ClientId { get; init; }
}
```

### Wolverine Handler

```csharp
public class CreateAppointmentHandler
{
    public async Task<CreateAppointmentResponse> Handle(
        CreateAppointmentCommand command)
    {
        // command.ExpertId is already populated from claims!
        // command.ExpertEmail is already populated from claims!

        var appointment = new Appointment
        {
            ExpertId = command.ExpertId,
            ExpertEmail = command.ExpertEmail,
            StartTime = command.StartTime,
            EndTime = command.EndTime,
            ClientId = command.ClientId
        };

        // Save appointment...
        return new CreateAppointmentResponse(appointment.Id);
    }
}
```

### HTTP Endpoint

```csharp
// The client only needs to send StartTime, EndTime, and ClientId
// ExpertId and ExpertEmail are automatically populated from the JWT
app.MapPost("/api/appointments",
    [Authorize]
    (CreateAppointmentCommand command) =>
    {
        // Command is automatically populated from claims
        return Results.Ok(command);
    });
```

### Example Request

```bash
POST /api/appointments
Authorization: Bearer <jwt_token>
Content-Type: application/json

{
  "startTime": "2026-02-15T10:00:00Z",
  "endTime": "2026-02-15T11:00:00Z",
  "clientId": "client-123"
}

# ExpertId and ExpertEmail are extracted from the JWT automatically
```

## Supported Property Types

The middleware automatically converts claim values to:
- `string`
- `Guid`
- `int`
- `long`
- `bool`
- `DateTime`
- `DateTimeOffset`
- Nullable versions of all above types

## Error Handling

### Required Claim Missing

If a required claim is missing:

```csharp
[FromClaim("department")] // Required = true by default
public required string Department { get; init; }
```

Throws: `UnauthorizedAccessException` with a clear message:
```
Required claim 'department' not found in user claims for property 'Department' on type 'CreateCourseCommand'.
User must be authenticated and have the required claim.
```

### Optional Claim

```csharp
[FromClaim("department", Required = false)]
public string? Department { get; init; }
```

No exception thrown; property remains `null` if claim not found.

## Best Practices

### 1. Use `[FromUserId]` for User Identity

```csharp
// Good
[FromUserId]
public required string CreatedBy { get; init; }

// Instead of manually accessing HttpContext
```

### 2. Make Claims Required When Needed

```csharp
// If the command MUST have the claim
[FromClaim("tenant_id", Required = true)]
public required string TenantId { get; init; }

// If the claim is optional
[FromClaim("preferences", Required = false)]
public string? Preferences { get; init; }
```

### 3. Use Meaningful Property Names

```csharp
// Good - clear intent
[FromUserId]
public required string InstructorId { get; init; }

// Avoid - confusing
[FromUserId]
public required string Id { get; init; }
```

### 4. Combine with Validation

```csharp
using FluentValidation;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        // InstructorId will be populated automatically, but still validate
        RuleFor(x => x.InstructorId)
            .NotEmpty()
            .WithMessage("Instructor ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}
```

## Integration with Wolverine HTTP

Complete example with Wolverine HTTP:

```csharp
using CodeClash.Identity.Extensions;
using Wolverine;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);

// Add authentication
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// Configure Wolverine
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    opts.UseClaimPopulation(); // Enable automatic claim population
});

builder.Services.AddWolverineHttp();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapWolverineEndpoints();

app.Run();
```

## Testing

### Unit Tests

```csharp
[Test]
public void Handler_ProcessesCommand_WithPopulatedClaims()
{
    var command = new CreateCourseCommand
    {
        InstructorId = "user-123", // Set manually in tests
        Title = "Test Course",
        Description = "Test Description"
    };

    var handler = new CreateCourseHandler();
    var result = await handler.Handle(command);

    result.Should().NotBeNull();
}
```

### Integration Tests

```csharp
[Test]
public async Task Endpoint_PopulatesClaims_FromAuthenticatedUser()
{
    var token = await GetValidJwtToken(); // Get real token

    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    var response = await client.PostAsJsonAsync("/api/courses", new
    {
        Title = "Test Course",
        Description = "Test Description"
        // InstructorId will be populated from token
    });

    response.Should().BeSuccessful();
}
```

## Troubleshooting

### Claims Not Being Populated

1. **Check authentication is enabled**
   ```csharp
   app.UseAuthentication(); // Must be called before UseAuthorization()
   app.UseAuthorization();
   ```

2. **Verify HttpContextAccessor is registered**
   ```csharp
   builder.Services.AddHttpContextAccessor();
   ```

3. **Ensure user is authenticated**
   - Add `[Authorize]` attribute or `.RequireAuthorization()`
   - Verify JWT token is valid

4. **Check claim exists in token**
   - Decode JWT at jwt.io
   - Verify the claim name matches exactly

### Wrong Claim Type

```csharp
// If using Keycloak, user ID might be in "sub" not "NameIdentifier"
[FromUserId] // Checks both automatically

// Or specify explicitly
[FromClaim(ClaimType = "sub", AlternateClaimType = ClaimTypes.NameIdentifier)]
public required string UserId { get; init; }
```
