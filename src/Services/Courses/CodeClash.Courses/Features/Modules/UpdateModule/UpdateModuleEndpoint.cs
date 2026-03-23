using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Modules.UpdateModule;

public sealed class UpdateModuleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPutWithAuth("/api/courses/{courseId}/modules/{moduleId}", Handle)
            .WithName("UpdateModule")
            .WithTags("Modules");
    }

    private static async Task<IResult> Handle(
        string courseId,
        string moduleId,
        UpdateModuleRequest request,
        IHttpContextAccessor httpContextAccessor,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext!.User.GetUserId()!;

        var command = new UpdateModuleCommand(
            courseId,
            moduleId,
            userId,
            request.Title,
            request.Description,
            request.Order,
            request.XpReward);

        return (await mediator.Send(command, cancellationToken)).ToNoContentProblemDetails();
    }
}

public sealed record UpdateModuleRequest(
    string? Title,
    string? Description,
    int? Order,
    int? XpReward);

public sealed record UpdateModuleCommand(
    string CourseId,
    string ModuleId,
    string AuthorId,
    string? Title,
    string? Description,
    int? Order,
    int? XpReward) : ICommand;

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
