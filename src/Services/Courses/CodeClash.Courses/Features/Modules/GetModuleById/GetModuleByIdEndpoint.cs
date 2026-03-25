using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Modules.GetModuleById;

public sealed class GetModuleByIdEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/courses/{courseId}/modules/{moduleId}", Handle)
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

public sealed record GetModuleByIdQuery(string CourseId, string ModuleId) : IQuery<ModuleDetailResponse>;

public sealed record ModuleDetailResponse(
    string ModuleId,
    string Title,
    string? Description,
    int Order,
    int XpReward,
    List<LessonListItem> Lessons);

public sealed record LessonListItem(
    string LessonId,
    string Title,
    LessonType Type,
    int Order);

public sealed class GetModuleByIdHandler(IMongoCollection<Course> courses)
    : IQueryHandler<GetModuleByIdQuery, ModuleDetailResponse>
{
    public async ValueTask<Result<ModuleDetailResponse>> Handle(
        GetModuleByIdQuery query, CancellationToken cancellationToken)
    {
        var course = await courses
            .Find(c => c.Id == query.CourseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result.Failure<ModuleDetailResponse>(CourseErrors.NotFound(query.CourseId));

        var module = course.Modules.FirstOrDefault(m => m.ModuleId == query.ModuleId);

        if (module is null)
            return Result.Failure<ModuleDetailResponse>(CourseErrors.ModuleNotFound(query.ModuleId));

        var response = new ModuleDetailResponse(
            module.ModuleId,
            module.Title,
            module.Description,
            module.Order,
            module.XpReward,
            module.Lessons
                .OrderBy(l => l.Order)
                .Select(l => new LessonListItem(l.LessonId, l.Title, l.Type, l.Order))
                .ToList());

        return response;
    }
}
