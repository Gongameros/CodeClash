using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Modules.AddModule;

public sealed class AddModuleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPostWithAuth("/api/courses/{courseId}/modules", Handle)
            .WithName("AddModule")
            .WithTags("Modules");
    }

    private static async Task<IResult> Handle(
        string courseId,
        AddModuleRequest request,
        IHttpContextAccessor httpContextAccessor,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext!.User.GetUserId()!;

        var command = new AddModuleCommand(
            courseId,
            userId,
            request.Title,
            request.Description,
            request.Order,
            request.XpReward);

        var result = await mediator.Send(command, cancellationToken);
        return result.ToCreatedProblemDetails($"/api/courses/{courseId}/modules/{result.Value.ModuleId}");
    }
}

public sealed record AddModuleRequest(
    string Title,
    string? Description,
    int Order,
    int XpReward);

public sealed record AddModuleCommand(
    string CourseId,
    string AuthorId,
    string Title,
    string? Description,
    int Order,
    int XpReward) : ICommand<AddModuleResponse>;

public sealed record AddModuleResponse(string ModuleId);

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
