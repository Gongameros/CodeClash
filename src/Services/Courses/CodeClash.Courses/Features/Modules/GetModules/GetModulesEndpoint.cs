using CodeClash.Results;
using CodeClash.Utilities.Endpoints;

namespace CodeClash.Courses.Features.Modules.GetModules;

public sealed class GetModulesEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGetWithAuth("/api/courses/{courseId}/modules", Handle)
            .WithName("GetModules")
            .WithTags("Modules");
    }

    private static async Task<IResult> Handle(
        string courseId,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        return (await mediator.Send(new GetModulesQuery(courseId), cancellationToken))
            .ToProblemDetails();
    }
}