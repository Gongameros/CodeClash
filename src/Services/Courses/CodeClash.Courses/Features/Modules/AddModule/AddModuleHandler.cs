using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Modules.AddModule;

public sealed class AddModuleHandler(IMongoCollection<Course> courses)
    : ICommandHandler<AddModuleCommand, AddModuleResponse>
{
    public async ValueTask<Result<AddModuleResponse>> Handle(
        AddModuleCommand command, CancellationToken cancellationToken)
    {
        var module = new CourseModule
        {
            Title = command.Title,
            Description = command.Description,
            Order = command.Order,
            XpReward = command.XpReward
        };

        var filter = Builders<Course>.Filter.Eq(c => c.Id, command.CourseId)
                     & Builders<Course>.Filter.Eq(c => c.AuthorId, command.AuthorId);

        var update = Builders<Course>.Update
            .Push(c => c.Modules, module)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await courses.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            return Result.Failure<AddModuleResponse>(CourseErrors.NotFound(command.CourseId));

        return new AddModuleResponse(module.ModuleId);
    }
}