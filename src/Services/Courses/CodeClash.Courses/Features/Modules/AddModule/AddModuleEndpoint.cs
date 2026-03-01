using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

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