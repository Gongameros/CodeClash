using CodeClash.Courses.Domains.Courses;
using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

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

public sealed record CreateCourseRequest(
    string Title,
    string Description,
    List<CodingTechnology> CodingTechnologies,
    CourseDifficulty Difficulty,
    List<string> Tags,
    string? ThumbnailUrl);

public sealed record CreateCourseCommand(
    string AuthorId,
    string Title,
    string Description,
    List<CodingTechnology> CodingTechnologies,
    CourseDifficulty Difficulty,
    List<string> Tags,
    string? ThumbnailUrl) : ICommand<CreateCourseResponse>;

public sealed record CreateCourseResponse(string Id);

public sealed class CreateCourseHandler(IMongoCollection<Course> courses)
    : ICommandHandler<CreateCourseCommand, CreateCourseResponse>
{
    public async ValueTask<Result<CreateCourseResponse>> Handle(
        CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var course = new Course
        {
            AuthorId = command.AuthorId,
            Title = command.Title,
            Description = command.Description,
            CodingTechnologies = command.CodingTechnologies,
            Difficulty = command.Difficulty,
            Tags = command.Tags,
            ThumbnailUrl = command.ThumbnailUrl
        };

        await courses.InsertOneAsync(course, cancellationToken: cancellationToken);

        return new CreateCourseResponse(course.Id);
    }
}
