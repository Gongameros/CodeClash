using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Lessons.UpdateLesson;

public sealed class UpdateLessonEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPutWithAuth("/api/courses/{courseId}/modules/{moduleId}/lessons/{lessonId}", Handle)
            .WithName("UpdateLesson")
            .WithTags("Lessons");
    }

    private static async Task<IResult> Handle(
        string courseId,
        string moduleId,
        string lessonId,
        UpdateLessonRequest request,
        IHttpContextAccessor httpContextAccessor,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext!.User.GetUserId()!;

        var command = new UpdateLessonCommand(
            courseId,
            moduleId,
            lessonId,
            userId,
            request.Title,
            request.Type,
            request.Order,
            request.Content,
            request.Challenge);

        return (await mediator.Send(command, cancellationToken)).ToNoContentProblemDetails();
    }
}