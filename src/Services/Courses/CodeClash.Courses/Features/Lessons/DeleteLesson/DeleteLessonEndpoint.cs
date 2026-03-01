using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Lessons.DeleteLesson;

public sealed class DeleteLessonEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDeleteWithAuth("/api/courses/{courseId}/modules/{moduleId}/lessons/{lessonId}", Handle)
            .WithName("DeleteLesson")
            .WithTags("Lessons");
    }

    private static async Task<IResult> Handle(
        string courseId,
        string moduleId,
        string lessonId,
        IHttpContextAccessor httpContextAccessor,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext!.User.GetUserId()!;
        return (await mediator.Send(new DeleteLessonCommand(courseId, moduleId, lessonId, userId), cancellationToken))
            .ToNoContentProblemDetails();
    }
}