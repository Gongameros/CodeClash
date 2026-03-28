# CodeClash.Identity Usage Examples

## Basic Setup

### 1. Add Project Reference

```xml
<ItemGroup>
  <ProjectReference Include="..\..\..\BuildingBlocks\CodeClash.Identity\CodeClash.Identity.csproj" />
</ItemGroup>
```

### 2. Configure appsettings.json

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

### 3. Update Program.cs

```csharp
using CodeClash.Identity.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Keycloak authentication
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// Add Swagger with OAuth2
builder.Services.AddSwaggerWithKeycloak(
    builder.Configuration,
    title: "My API",
    version: "v1");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithKeycloak(
        builder.Configuration,
        clientId: "swagger-ui");
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

## Authorization Examples

### Require Authentication

```csharp
app.MapGet("/api/protected", () => "Protected data")
    .RequireAuthorization();
```

### Require Specific Role

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

### Custom Policy

```csharp
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InstructorOnly", policy =>
        policy.RequireRole("instructor"));

    options.AddPolicy("VerifiedEmail", policy =>
        policy.RequireClaim("email_verified", "true"));
});

// In endpoint
app.MapGet("/api/courses", () => "Courses")
    .RequireAuthorization("InstructorOnly");
```

## Accessing User Information

### Using ClaimsPrincipal Extensions

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
        FirstName = user.GetFirstName(),
        LastName = user.GetLastName(),
        Roles = user.GetRoles()
    };
})
.RequireAuthorization();
```

### Manual Claims Access

```csharp
app.MapGet("/api/user-info", (ClaimsPrincipal user) =>
{
    var claims = user.Claims.Select(c => new
    {
        Type = c.Type,
        Value = c.Value
    });

    return Results.Ok(claims);
})
.RequireAuthorization();
```

### In Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProfile()
    {
        var userId = User.GetUserId();
        var username = User.GetUsername();
        var email = User.GetEmail();

        return Ok(new { userId, username, email });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "admin")]
    public IActionResult AdminOnly()
    {
        return Ok("Admin access granted");
    }
}
```

## Advanced Scenarios

### Custom Authorization Handler

```csharp
// Custom requirement
public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }

    public MinimumAgeRequirement(int minimumAge)
    {
        MinimumAge = minimumAge;
    }
}

// Custom handler
public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var dateOfBirthClaim = context.User.FindFirst(c => c.Type == "date_of_birth");

        if (dateOfBirthClaim == null)
        {
            return Task.CompletedTask;
        }

        var dateOfBirth = Convert.ToDateTime(dateOfBirthClaim.Value);
        var age = DateTime.Today.Year - dateOfBirth.Year;

        if (dateOfBirth > DateTime.Today.AddYears(-age))
        {
            age--;
        }

        if (age >= requirement.MinimumAge)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

// Register in Program.cs
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AtLeast18", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

// Use in endpoint
app.MapGet("/api/adult-only", () => "18+ content")
    .RequireAuthorization("AtLeast18");
```

### Resource-Based Authorization

```csharp
public class DocumentAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Document resource)
    {
        var userId = context.User.GetUserId();

        if (requirement.Name == "Edit" && resource.OwnerId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

// In endpoint
app.MapPut("/api/documents/{id}", async (
    int id,
    IAuthorizationService authService,
    ClaimsPrincipal user) =>
{
    var document = await GetDocumentById(id);

    var authResult = await authService.AuthorizeAsync(
        user,
        document,
        "Edit");

    if (!authResult.Succeeded)
    {
        return Results.Forbid();
    }

    // Update document
    return Results.Ok();
});
```

### Accessing Claims in Services

```csharp
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.GetUserId();

    public string? Username =>
        _httpContextAccessor.HttpContext?.User.GetUsername();

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.GetEmail();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}

// Register in Program.cs
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Use in services
public class CourseService
{
    private readonly ICurrentUserService _currentUser;

    public CourseService(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<Course> CreateCourse(CreateCourseRequest request)
    {
        var course = new Course
        {
            Title = request.Title,
            CreatedBy = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };

        // Save course...
        return course;
    }
}
```

## Testing with Authentication

### Integration Tests

```csharp
public class AuthenticatedApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthenticatedApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/protected");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsOk()
    {
        var token = await GetValidToken();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/protected");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<string> GetValidToken()
    {
        // Get token from Keycloak or use test token
        // Implementation depends on your test strategy
        return "test-token";
    }
}
```

## Environment Configuration

### Development (appsettings.Development.json)

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

### Production (appsettings.Production.json)

```json
{
  "Keycloak": {
    "Authority": "https://keycloak.production.com",
    "Realm": "codeclash",
    "Audience": "account",
    "RequireHttpsMetadata": true
  }
}
```

### Using Environment Variables

```bash
export Keycloak__Authority=http://localhost:8080
export Keycloak__Realm=codeclash
export Keycloak__Audience=account
```
