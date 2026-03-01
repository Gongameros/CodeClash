using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Courses.GetCourseById;

public sealed class GetCourseByIdEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGetWithAuth("/api/courses/{courseId}", Handle)
            .WithName("GetCourseById")
            .WithTags("Courses");
    }

    private static async Task<IResult> Handle(
        string courseId,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        return (await mediator.Send(new GetCourseByIdQuery(courseId), cancellationToken))
            .ToProblemDetails();
    }
}