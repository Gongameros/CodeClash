using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace CodeClash.Courses.Features.Courses.GetCourses;

public sealed class GetCoursesEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGetWithAuth("/api/courses", Handle)
            .WithName("GetCourses")
            .WithTags("Courses");
    }

    private static async Task<IResult> Handle(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        return (await mediator.Send(new GetCoursesQuery(page, pageSize), cancellationToken))
            .ToProblemDetails();
    }
}