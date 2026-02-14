# Simple Claim Population

Automatically populate command/request properties from authenticated user claims.

## Quick Start

### 1. Define Your Command with Attributes

```csharp
using CodeClash.Identity.Attributes;
using System.Security.Claims;

public class CreateAppointmentCommand
{
    // Automatically populated from user ID claim
    [FromUserId]
    public required string ExpertId { get; init; }

    // From request
    public required DateTime StartTime { get; init; }
    public required string ClientId { get; init; }
}
```

### 2. Use in Your Handler/Endpoint

```csharp
using CodeClash.Identity.Services;

app.MapPost("/api/appointments",
    [Authorize]
    async (CreateAppointmentCommand command,
           IClaimPopulationService claimService,
           ClaimsPrincipal user) =>
    {
        // Populate claims before processing
        claimService.PopulateFromClaims(command, user);

        // Now command.ExpertId is set from the JWT token
        // Process the command...
        return Results.Ok();
    });
```

## Available Attributes

### `[FromUserId]` - Get User ID

```csharp
[FromUserId]
public required string UserId { get; init; }
```

Automatically extracts from `sub` or `NameIdentifier` claim.

### `[FromClaim]` - Get Specific Claim

```csharp
// By claim type
[FromClaim(ClaimTypes.Email)]
public required string Email { get; init; }

// By custom claim name
[FromClaim("tenant_id")]
public required string TenantId { get; init; }

// Optional claim (no error if missing)
[FromClaim("department", Required = false)]
public string? Department { get; init; }

// With fallback
[FromClaim(ClaimType = "org_id", AlternateClaimType = "organization_id")]
public required string OrgId { get; init; }
```

## Complete Example

```csharp
using CodeClash.Identity.Attributes;
using CodeClash.Identity.Services;
using System.Security.Claims;

// Command
public class CreateCourseCommand
{
    [FromUserId]
    public required string InstructorId { get; init; }

    [FromClaim(ClaimTypes.Email)]
    public required string InstructorEmail { get; init; }

    public required string Title { get; init; }
    public required string Description { get; init; }
}

// Handler
public class CourseService
{
    private readonly IClaimPopulationService _claimService;

    public CourseService(IClaimPopulationService claimService)
    {
        _claimService = claimService;
    }

    public async Task<CourseResponse> CreateCourse(
        CreateCourseCommand command,
        ClaimsPrincipal user)
    {
        // Populate claims
        _claimService.PopulateFromClaims(command, user);

        // command.InstructorId and InstructorEmail are now set
        var course = new Course
        {
            InstructorId = command.InstructorId,
            InstructorEmail = command.InstructorEmail,
            Title = command.Title,
            Description = command.Description
        };

        // Save course...
        return new CourseResponse(course.Id);
    }
}

// Endpoint
app.MapPost("/api/courses",
    [Authorize]
    async (CreateCourseCommand command,
           IClaimPopulationService claimService,
           ClaimsPrincipal user) =>
    {
        claimService.PopulateFromClaims(command, user);
        // Process command...
        return Results.Created($"/api/courses/{courseId}", course);
    });
```

## With Action Filter (Controllers)

If using controllers, the filter automatically populates claims:

```csharp
using CodeClash.Identity.Extensions;

// In Program.cs
builder.Services.AddControllers();
builder.Services.AddClaimPopulation(); // Adds the filter

// In Controller
[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateCourseCommand command)
    {
        // command.InstructorId is automatically populated by the filter
        return Ok();
    }
}
```

## Supported Types

- `string`
- `Guid`
- `int`, `long`
- `bool`
- `DateTime`, `DateTimeOffset`
- Nullable versions of all above

## Error Handling

### Required Claim Missing

```csharp
[FromUserId] // Required by default
public required string UserId { get; init; }
```

Throws `UnauthorizedAccessException` with message:
```
Required claim 'sub' not found in user claims for property 'UserId' on type 'MyCommand'.
User must be authenticated and have the required claim.
```

### Optional Claim

```csharp
[FromClaim("preferences", Required = false)]
public string? Preferences { get; init; }
```

No exception; property stays `null` if claim not found.

## Testing

```csharp
[Test]
public void Service_PopulatesClaims()
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, "user-123"),
        new Claim(ClaimTypes.Email, "user@example.com")
    };
    var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

    var command = new CreateCourseCommand
    {
        Title = "Test",
        Description = "Test"
    };

    var service = new ClaimPopulationService();
    service.PopulateFromClaims(command, user);

    Assert.That(command.InstructorId, Is.EqualTo("user-123"));
    Assert.That(command.InstructorEmail, Is.EqualTo("user@example.com"));
}
```

## Best Practices

1. **Always use with `[Authorize]`** - Claims only available for authenticated users
2. **Validate after population** - Use FluentValidation or similar
3. **Use meaningful names** - `InstructorId` better than just `Id`
4. **Make security claims required** - Don't allow missing user IDs
5. **Test with real tokens** - Ensure claim names match your Keycloak setup
