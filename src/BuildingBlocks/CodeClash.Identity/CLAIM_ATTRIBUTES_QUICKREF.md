# Claim Attributes - Quick Reference

## What You Asked For

You wanted the ability to automatically extract `coderId` (or any user claim) from the JWT token in your request commands. This is now available! ✅

## How to Use

### 1. Mark Properties in Your Command

```csharp
using CodeClash.Identity.Attributes;
using System.Security.Claims;

public class CheckAvailabilityCommand
{
    // Automatically populated from user's ID in JWT
    [FromUserId]
    public required string ExpertId { get; init; }

    // From request body/query
    public required DateTime StartTime { get; init; }
}
```

### 2. Populate in Your Handler

```csharp
using CodeClash.Identity.Services;

public class YourHandler
{
    private readonly IClaimPopulationService _claimService;

    public YourHandler(IClaimPopulationService claimService)
    {
        _claimService = claimService;
    }

    public async Task Handle(CheckAvailabilityCommand command, ClaimsPrincipal user)
    {
        // Automatically populate claims
        _claimService.PopulateFromClaims(command, user);

        // command.ExpertId is now set from the JWT!
        Console.WriteLine($"Expert: {command.ExpertId}");
    }
}
```

### 3. Or in Minimal API

```csharp
app.MapPost("/api/check-availability",
    [Authorize]
    async (CheckAvailabilityCommand command,
           IClaimPopulationService claimService,
           ClaimsPrincipal user) =>
    {
        claimService.PopulateFromClaims(command, user);
        // command.ExpertId is set!
        return Results.Ok();
    });
```

## Available Attributes

| Attribute | Usage | Description |
|-----------|-------|-------------|
| `[FromUserId]` | `[FromUserId]` | Gets user ID from `sub` or `NameIdentifier` claim |
| `[FromClaim]` | `[FromClaim(ClaimTypes.Email)]` | Gets specific claim by type |
| `[FromClaim]` | `[FromClaim("custom_claim")]` | Gets custom claim by name |
| `[FromClaim]` | `[FromClaim("dept", Required = false)]` | Optional claim (no error if missing) |

## Complete Example Matching Your Request

```csharp
using CodeClash.Identity.Attributes;
using System.Security.Claims;

// Your command
public class CheckAvailabilityOfAppointmentSlotCommand
{
    [FromClaim(ClaimTypes.NameIdentifier)]  // or [FromUserId]
    public required string ExpertId { get; init; }

    public required DateTime StartTime { get; init; }
}

// Your handler
public class AppointmentHandler
{
    private readonly IClaimPopulationService _claimService;

    public AppointmentHandler(IClaimPopulationService claimService)
    {
        _claimService = claimService;
    }

    public async Task<bool> Handle(
        CheckAvailabilityOfAppointmentSlotCommand command,
        ClaimsPrincipal user)
    {
        // Populate ExpertId from JWT token
        _claimService.PopulateFromClaims(command, user);

        // Now command.ExpertId contains the authenticated user's ID
        // Check availability logic...
        return true;
    }
}
```

## What Gets Set Automatically

When a user sends a request with a JWT token:

```bash
POST /api/check-availability
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
  "startTime": "2026-02-15T10:00:00Z"
}
```

After calling `PopulateFromClaims()`:
- ✅ `ExpertId` is extracted from the JWT `sub` or `NameIdentifier` claim
- ✅ `StartTime` comes from the request body
- ✅ User never needs to send their own ID (prevents spoofing!)

## Security Benefits

1. **Prevents ID Spoofing** - Users can't fake their ID in requests
2. **Automatic Validation** - Required claims throw errors if missing
3. **Type Safety** - Supports string, Guid, int, DateTime, etc.
4. **Clear Intent** - Code explicitly shows what comes from claims

## More Info

- [CLAIMS_SIMPLE.md](./CLAIMS_SIMPLE.md) - Complete guide
- [Examples/CommandExample.cs](./Examples/CommandExample.cs) - Code examples
