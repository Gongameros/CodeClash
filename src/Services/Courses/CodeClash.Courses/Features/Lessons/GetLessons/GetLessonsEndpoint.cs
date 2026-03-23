using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Lessons.GetLessons;

public sealed class GetLessonsEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/courses/{courseId}/modules/{moduleId}/lessons", Handle)
            .WithName("GetLessons")
            .WithTags("Lessons");
    }

    private static async Task<IResult> Handle(
        string courseId,
        string moduleId,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        return (await mediator.Send(new GetLessonsQuery(courseId, moduleId), cancellationToken))
            .ToProblemDetails();
    }
}

public sealed record GetLessonsQuery(string CourseId, string ModuleId)
    : IQuery<IReadOnlyList<LessonListItem>>;

public sealed record LessonListItem(
    string LessonId,
    string Title,
    LessonType Type,
    int Order);

public sealed class GetLessonsHandler(IMongoCollection<Course> courses)
    : IQueryHandler<GetLessonsQuery, IReadOnlyList<LessonListItem>>
{
    public async ValueTask<Result<IReadOnlyList<LessonListItem>>> Handle(
        GetLessonsQuery query, CancellationToken cancellationToken)
    {
        var course = await courses
            .Find(c => c.Id == query.CourseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result.Failure<IReadOnlyList<LessonListItem>>(CourseErrors.NotFound(query.CourseId));

        var module = course.Modules.FirstOrDefault(m => m.ModuleId == query.ModuleId);

        if (module is null)
            return Result.Failure<IReadOnlyList<LessonListItem>>(CourseErrors.ModuleNotFound(query.ModuleId));

        IReadOnlyList<LessonListItem> lessons = module.Lessons
            .OrderBy(l => l.Order)
            .Select(l => new LessonListItem(l.LessonId, l.Title, l.Type, l.Order))
            .ToList()
            .AsReadOnly();

        return Result.Success(lessons);
    }
}
