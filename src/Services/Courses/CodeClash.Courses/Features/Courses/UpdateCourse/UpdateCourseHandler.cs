using CodeClash.Courses.Domains.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Courses.UpdateCourse;

public sealed class UpdateCourseHandler(IMongoCollection<Course> courses)
    : ICommandHandler<UpdateCourseCommand>
{
    public async ValueTask<Result<Result>> Handle(
        UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        var filter = Builders<Course>.Filter.Eq(c => c.Id, command.CourseId)
                     & Builders<Course>.Filter.Eq(c => c.AuthorId, command.AuthorId);

        var updates = new List<UpdateDefinition<Course>>();

        if (command.Title is not null)
            updates.Add(Builders<Course>.Update.Set(c => c.Title, command.Title));
        if (command.Description is not null)
            updates.Add(Builders<Course>.Update.Set(c => c.Description, command.Description));
        if (command.CodingTechnologies is not null)
            updates.Add(Builders<Course>.Update.Set(c => c.CodingTechnologies, command.CodingTechnologies));
        if (command.Difficulty is not null)
            updates.Add(Builders<Course>.Update.Set(c => c.Difficulty, command.Difficulty.Value));
        if (command.Tags is not null)
            updates.Add(Builders<Course>.Update.Set(c => c.Tags, command.Tags));
        if (command.ThumbnailUrl is not null)
            updates.Add(Builders<Course>.Update.Set(c => c.ThumbnailUrl, command.ThumbnailUrl));
        if (command.IsPublished is not null)
            updates.Add(Builders<Course>.Update.Set(c => c.IsPublished, command.IsPublished.Value));

        if (updates.Count == 0)
            return Result.Success();

        updates.Add(Builders<Course>.Update.Set(c => c.UpdatedAt, DateTime.UtcNow));

        var result = await courses.UpdateOneAsync(
            filter,
            Builders<Course>.Update.Combine(updates),
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            return Result.Failure<Result>(CourseErrors.NotFound(command.CourseId));

        return Result.Success();
    }
}