using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

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

public sealed record AddLessonRequest(
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallenge? Challenge);

public sealed record AddLessonCommand(
    string CourseId,
    string ModuleId,
    string AuthorId,
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallenge? Challenge) : ICommand<AddLessonResponse>;

public sealed record AddLessonResponse(string LessonId);

public sealed class AddLessonHandler(IMongoCollection<Course> courses)
    : ICommandHandler<AddLessonCommand, AddLessonResponse>
{
    public async ValueTask<Result<AddLessonResponse>> Handle(
        AddLessonCommand command, CancellationToken cancellationToken)
    {
        var lesson = new Lesson
        {
            Title = command.Title,
            Type = command.Type,
            Order = command.Order,
            Content = command.Content,
            Challenge = command.Challenge
        };

        var filter = Builders<Course>.Filter.Eq(c => c.Id, command.CourseId)
                     & Builders<Course>.Filter.Eq(c => c.AuthorId, command.AuthorId)
                     & Builders<Course>.Filter.ElemMatch(c => c.Modules,
                         m => m.ModuleId == command.ModuleId);

        var update = Builders<Course>.Update
            .Push("modules.$.lessons", lesson)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await courses.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            return Result.Failure<AddLessonResponse>(CourseErrors.ModuleNotFound(command.ModuleId));

        return new AddLessonResponse(lesson.LessonId);
    }
}
