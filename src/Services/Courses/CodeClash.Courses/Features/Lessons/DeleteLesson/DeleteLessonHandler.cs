using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Lessons.DeleteLesson;

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
            .PullFilter("modules.$.lessons",
                Builders<Lesson>.Filter.Eq(l => l.LessonId, command.LessonId))
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await courses.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            return Result.Failure<Result>(CourseErrors.ModuleNotFound(command.ModuleId));

        if (result.ModifiedCount == 0)
            return Result.Failure<Result>(CourseErrors.LessonNotFound(command.LessonId));

        return Result.Success();
    }
}