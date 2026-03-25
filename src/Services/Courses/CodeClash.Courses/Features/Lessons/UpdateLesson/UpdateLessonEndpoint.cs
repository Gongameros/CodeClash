using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Bson;
using MongoDB.Driver;

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

public sealed record UpdateLessonRequest(
    string? Title,
    LessonType? Type,
    int? Order,
    string? Content,
    CodingChallenge? Challenge);

public sealed record UpdateLessonCommand(
    string CourseId,
    string ModuleId,
    string LessonId,
    string AuthorId,
    string? Title,
    LessonType? Type,
    int? Order,
    string? Content,
    CodingChallenge? Challenge) : ICommand;

public sealed class UpdateLessonHandler(IMongoCollection<Course> courses)
    : ICommandHandler<UpdateLessonCommand>
{
    public async ValueTask<Result<Result>> Handle(
        UpdateLessonCommand command, CancellationToken cancellationToken)
    {
        var filter = Builders<Course>.Filter.Eq(c => c.Id, command.CourseId)
                     & Builders<Course>.Filter.Eq(c => c.AuthorId, command.AuthorId);

        var updates = new List<UpdateDefinition<Course>>();

        if (command.Title is not null)
            updates.Add(Builders<Course>.Update.Set("modules.$[m].lessons.$[l].title", command.Title));
        if (command.Type is not null)
            updates.Add(Builders<Course>.Update.Set("modules.$[m].lessons.$[l].type", command.Type.Value));
        if (command.Order is not null)
            updates.Add(Builders<Course>.Update.Set("modules.$[m].lessons.$[l].order", command.Order.Value));
        if (command.Content is not null)
            updates.Add(Builders<Course>.Update.Set("modules.$[m].lessons.$[l].content", command.Content));
        if (command.Challenge is not null)
            updates.Add(Builders<Course>.Update.Set("modules.$[m].lessons.$[l].challenge", command.Challenge));

        if (updates.Count == 0)
            return Result.Success();

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new BsonDocumentArrayFilterDefinition<Course>(
                new BsonDocument("m.moduleId", new ObjectId(command.ModuleId))),
            new BsonDocumentArrayFilterDefinition<Course>(
                new BsonDocument("l.lessonId", new ObjectId(command.LessonId)))
        };

        var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

        var result = await courses.UpdateOneAsync(
            filter,
            Builders<Course>.Update.Combine(updates),
            updateOptions,
            cancellationToken);

        if (result.MatchedCount == 0)
            return Result.Failure<Result>(CourseErrors.NotFound(command.CourseId));

        if (result.ModifiedCount == 0)
            return Result.Failure<Result>(CourseErrors.LessonNotFound(command.LessonId));

        await courses.UpdateOneAsync(
            Builders<Course>.Filter.Eq(c => c.Id, command.CourseId),
            Builders<Course>.Update.Set(c => c.UpdatedAt, DateTime.UtcNow),
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
