using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Lessons.UpdateLesson;

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

        updates.Add(Builders<Course>.Update.Set(c => c.UpdatedAt, DateTime.UtcNow));

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new BsonDocumentArrayFilterDefinition<Course>(
                global::MongoDB.Bson.BsonDocument.Parse($"{{'m.moduleId': '{command.ModuleId}'}}")),
            new BsonDocumentArrayFilterDefinition<Course>(
                global::MongoDB.Bson.BsonDocument.Parse($"{{'l.lessonId': '{command.LessonId}'}}"))
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

        return Result.Success();
    }
}