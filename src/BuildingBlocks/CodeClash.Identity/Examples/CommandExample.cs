using System.Security.Claims;
using CodeClash.Identity.Attributes;
using CodeClash.Identity.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CodeClash.Identity.Examples;

// Example 1: Simple command with user ID
public class CreateCourseCommand
{
    [FromUserId]
    public required string InstructorId { get; init; }

    public required string Title { get; init; }
    public required string Description { get; init; }
}

// Example 2: Command with multiple claims
public class CreateAppointmentCommand
{
    [FromUserId]
    public required string ExpertId { get; init; }

    [FromClaim(ClaimTypes.Email)]
    public required string ExpertEmail { get; init; }

    [FromClaim("timezone", Required = false)]
    public string? Timezone { get; init; }

    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }
}

// Example 3: Handler usage
public class CourseHandler
{
    private readonly IClaimPopulationService _claimService;

    public CourseHandler(IClaimPopulationService claimService)
    {
        _claimService = claimService;
    }

    public async Task<string> Handle(CreateCourseCommand command, ClaimsPrincipal user)
    {
        // Populate claims before processing
        _claimService.PopulateFromClaims(command, user);

        // Now command.InstructorId is automatically set from the JWT token
        Console.WriteLine($"Creating course for instructor: {command.InstructorId}");

        // Your business logic here
        return "course-123";
    }
}

// Example 4: Minimal API endpoint
public static class EndpointExamples
{
    public static void MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/courses",
            async (CreateCourseCommand command,
                   IClaimPopulationService claimService,
                   ClaimsPrincipal user) =>
            {
                // Populate claims from authenticated user
                claimService.PopulateFromClaims(command, user);

                // Process command - InstructorId is now set
                // ... your logic here

                return Results.Created($"/api/courses/{Guid.NewGuid()}", command);
            })
            .RequireAuthorization();
    }
}
