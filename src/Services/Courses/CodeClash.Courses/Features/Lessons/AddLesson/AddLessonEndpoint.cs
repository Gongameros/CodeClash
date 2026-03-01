using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Lessons.AddLesson;

public sealed class AddLessonEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPostWithAuth("/api/courses/{courseId}/modules/{moduleId}/lessons", Handle)
            .WithName("AddLesson")
            .WithTags("Lessons");
    }

    private static async Task<IResult> Handle(
        string courseId,
        string moduleId,
        AddLessonRequest request,
        IHttpContextAccessor httpContextAccessor,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext!.User.GetUserId()!;

        var command = new AddLessonCommand(
            courseId,
            moduleId,
            userId,
            request.Title,
            request.Type,
            request.Order,
            request.Content,
            request.Challenge);

        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedProblemDetails(
            $"/api/courses/{courseId}/modules/{moduleId}/lessons/{result.Value.LessonId}");
    }
}