using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

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