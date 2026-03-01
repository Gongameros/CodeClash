using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Lessons.AddLesson;

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