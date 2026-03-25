using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Modules.GetModules;

public sealed class GetModulesEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/courses/{courseId}/modules", Handle)
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

public sealed record GetModulesQuery(string CourseId) : IQuery<IReadOnlyList<ModuleListItem>>;

public sealed record ModuleListItem(
    string ModuleId,
    string Title,
    string? Description,
    int Order,
    int XpReward,
    int LessonCount);

public sealed class GetModulesHandler(IMongoCollection<Course> courses)
    : IQueryHandler<GetModulesQuery, IReadOnlyList<ModuleListItem>>
{
    public async ValueTask<Result<IReadOnlyList<ModuleListItem>>> Handle(
        GetModulesQuery query, CancellationToken cancellationToken)
    {
        var course = await courses
            .Find(c => c.Id == query.CourseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result.Failure<IReadOnlyList<ModuleListItem>>(CourseErrors.NotFound(query.CourseId));

        IReadOnlyList<ModuleListItem> modules = course.Modules
            .OrderBy(m => m.Order)
            .Select(m => new ModuleListItem(
                m.ModuleId,
                m.Title,
                m.Description,
                m.Order,
                m.XpReward,
                m.Lessons.Count))
            .ToList()
            .AsReadOnly();

        return Result.Success(modules);
    }
}
