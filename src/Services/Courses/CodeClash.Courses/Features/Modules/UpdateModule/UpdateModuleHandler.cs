using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Modules.UpdateModule;

public sealed class UpdateModuleHandler(IMongoCollection<Course> courses)
    : ICommandHandler<UpdateModuleCommand>
{
    public async ValueTask<Result<Result>> Handle(
        UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        var filter = Builders<Course>.Filter.Eq(c => c.Id, command.CourseId)
                     & Builders<Course>.Filter.Eq(c => c.AuthorId, command.AuthorId)
                     & Builders<Course>.Filter.ElemMatch(c => c.Modules,
                         m => m.ModuleId == command.ModuleId);

        var updates = new List<UpdateDefinition<Course>>();

        if (command.Title is not null)
            updates.Add(Builders<Course>.Update.Set("modules.$.title", command.Title));
        if (command.Description is not null)
            updates.Add(Builders<Course>.Update.Set("modules.$.description", command.Description));
        if (command.Order is not null)
            updates.Add(Builders<Course>.Update.Set("modules.$.order", command.Order.Value));
        if (command.XpReward is not null)
            updates.Add(Builders<Course>.Update.Set("modules.$.xpReward", command.XpReward.Value));

        if (updates.Count == 0)
            return Result.Success();

        updates.Add(Builders<Course>.Update.Set(c => c.UpdatedAt, DateTime.UtcNow));

        var result = await courses.UpdateOneAsync(
            filter,
            Builders<Course>.Update.Combine(updates),
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            return Result.Failure<Result>(CourseErrors.ModuleNotFound(command.ModuleId));

        return Result.Success();
    }
}