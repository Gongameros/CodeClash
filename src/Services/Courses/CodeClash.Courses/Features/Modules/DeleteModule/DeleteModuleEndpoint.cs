using CodeClash.Identity.Extensions;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Modules.DeleteModule;

public sealed class DeleteModuleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDeleteWithAuth("/api/courses/{courseId}/modules/{moduleId}", Handle)
            .WithName("DeleteModule")
            .WithTags("Modules");
    }

    private static async Task<IResult> Handle(
        string courseId,
        string moduleId,
        IHttpContextAccessor httpContextAccessor,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext!.User.GetUserId()!;
        return (await mediator.Send(new DeleteModuleCommand(courseId, moduleId, userId), cancellationToken))
            .ToNoContentProblemDetails();
    }
}