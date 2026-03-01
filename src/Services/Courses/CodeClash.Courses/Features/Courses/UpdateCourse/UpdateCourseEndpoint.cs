using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Courses.UpdateCourse;

public sealed class UpdateCourseEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPutWithAuth("/api/courses/{courseId}", Handle)
            .WithName("UpdateCourse")
            .WithTags("Courses");
    }

    private static async Task<IResult> Handle(
        string courseId,
        UpdateCourseRequest request,
        IHttpContextAccessor httpContextAccessor,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext!.User.GetUserId()!;

        var command = new UpdateCourseCommand(
            courseId,
            userId,
            request.Title,
            request.Description,
            request.CodingTechnologies,
            request.Difficulty,
            request.Tags,
            request.ThumbnailUrl,
            request.IsPublished);

        return (await mediator.Send(command, cancellationToken)).ToNoContentProblemDetails();
    }
}