using CodeClash.Courses.Domains.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Courses.DeleteCourse;

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