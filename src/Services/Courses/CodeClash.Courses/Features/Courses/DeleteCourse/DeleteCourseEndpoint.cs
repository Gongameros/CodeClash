using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Courses.DeleteCourse;

public sealed class DeleteCourseEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDeleteWithAuth("/api/courses/{courseId}", Handle)
            .WithName("DeleteCourse")
            .WithTags("Courses");
    }

    private static async Task<IResult> Handle(
        string courseId,
        IHttpContextAccessor httpContextAccessor,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext!.User.GetUserId()!;
        return (await mediator.Send(new DeleteCourseCommand(courseId, userId), cancellationToken))
            .ToNoContentProblemDetails();
    }
}
