using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Lessons.GetLessons;

public sealed class GetLessonsEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGetWithAuth("/api/courses/{courseId}/modules/{moduleId}/lessons", Handle)
            .WithName("GetLessons")
            .WithTags("Lessons");
    }

    private static async Task<IResult> Handle(
        string courseId,
        string moduleId,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        return (await mediator.Send(new GetLessonsQuery(courseId, moduleId), cancellationToken))
            .ToProblemDetails();
    }
}