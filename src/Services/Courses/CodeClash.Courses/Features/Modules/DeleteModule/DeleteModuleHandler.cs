using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Modules.DeleteModule;

public sealed class DeleteModuleHandler(IMongoCollection<Course> courses)
    : ICommandHandler<DeleteModuleCommand>
{
    public async ValueTask<Result<Result>> Handle(
        DeleteModuleCommand command, CancellationToken cancellationToken)
    {
        var filter = Builders<Course>.Filter.Eq(c => c.Id, command.CourseId)
                     & Builders<Course>.Filter.Eq(c => c.AuthorId, command.AuthorId);

        var update = Builders<Course>.Update
            .PullFilter(c => c.Modules, m => m.ModuleId == command.ModuleId)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await courses.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            return Result.Failure<Result>(CourseErrors.NotFound(command.CourseId));

        if (result.ModifiedCount == 0)
            return Result.Failure<Result>(CourseErrors.ModuleNotFound(command.ModuleId));

        return Result.Success();
    }
}