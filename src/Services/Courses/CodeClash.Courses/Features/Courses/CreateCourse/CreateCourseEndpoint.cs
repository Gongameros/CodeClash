using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Courses.CreateCourse;

public sealed class CreateCourseEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPostWithAuth("/api/courses", Handle)
            .WithName("CreateCourse")
            .WithTags("Courses");
    }

    private static async Task<IResult> Handle(
        CreateCourseRequest request,
        IHttpContextAccessor httpContextAccessor,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext!.User.GetUserId()!;

        var command = new CreateCourseCommand(
            userId,
            request.Title,
            request.Description,
            request.CodingTechnologies,
            request.Difficulty,
            request.Tags,
            request.ThumbnailUrl);

        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedProblemDetails($"/api/courses/{result.Value.Id}");
    }
}