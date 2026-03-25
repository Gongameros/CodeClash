using CodeClash.Courses.Domains.Courses;
using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

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

public sealed record DeleteCourseCommand(string CourseId, string AuthorId) : ICommand;

public sealed class DeleteCourseHandler(IMongoCollection<Course> courses)
    : ICommandHandler<DeleteCourseCommand>
{
    public async ValueTask<Result<Result>> Handle(
        DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        var filter = Builders<Course>.Filter.Eq(c => c.Id, command.CourseId)
                     & Builders<Course>.Filter.Eq(c => c.AuthorId, command.AuthorId);

        var result = await courses.DeleteOneAsync(filter, cancellationToken);

        if (result.DeletedCount == 0)
            return Result.Failure<Result>(CourseErrors.NotFound(command.CourseId));

        return Result.Success();
    }
}
