using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Modules.GetModuleById;

public sealed class GetModuleByIdEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGetWithAuth("/api/courses/{courseId}/modules/{moduleId}", Handle)
            .WithName("GetModuleById")
            .WithTags("Modules");
    }

    private static async Task<IResult> Handle(
        string courseId,
        string moduleId,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        return (await mediator.Send(new GetModuleByIdQuery(courseId, moduleId), cancellationToken))
            .ToProblemDetails();
    }
}