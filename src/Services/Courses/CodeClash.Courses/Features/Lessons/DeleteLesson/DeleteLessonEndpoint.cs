using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

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

public sealed record DeleteLessonCommand(
    string CourseId,
    string ModuleId,
    string LessonId,
    string AuthorId) : ICommand;

public sealed class DeleteLessonHandler(IMongoCollection<Course> courses)
    : ICommandHandler<DeleteLessonCommand>
{
    public async ValueTask<Result<Result>> Handle(
        DeleteLessonCommand command, CancellationToken cancellationToken)
    {
        var filter = Builders<Course>.Filter.Eq(c => c.Id, command.CourseId)
                     & Builders<Course>.Filter.Eq(c => c.AuthorId, command.AuthorId)
                     & Builders<Course>.Filter.ElemMatch(c => c.Modules,
                         m => m.ModuleId == command.ModuleId);

        var update = Builders<Course>.Update
            .PullFilter("modules.$.lessons", Builders<Lesson>.Filter.Eq(l => l.LessonId, command.LessonId));

        var result = await courses.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            return Result.Failure<Result>(CourseErrors.ModuleNotFound(command.ModuleId));

        if (result.ModifiedCount == 0)
            return Result.Failure<Result>(CourseErrors.LessonNotFound(command.LessonId));

        return Result.Success();
    }
}
