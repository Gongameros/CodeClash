using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Lessons.GetLessonById;

public sealed class GetLessonByIdEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/courses/{courseId}/modules/{moduleId}/lessons/{lessonId}", Handle)
            .WithName("GetLessonById")
            .WithTags("Lessons");
    }

    private static async Task<IResult> Handle(
        string courseId,
        string moduleId,
        string lessonId,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        return (await mediator.Send(new GetLessonByIdQuery(courseId, moduleId, lessonId), cancellationToken))
            .ToProblemDetails();
    }
}

public sealed record GetLessonByIdQuery(string CourseId, string ModuleId, string LessonId)
    : IQuery<LessonDetailResponse>;

public sealed record LessonDetailResponse(
    string LessonId,
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallenge? Challenge);

public sealed class GetLessonByIdHandler(IMongoCollection<Course> courses)
    : IQueryHandler<GetLessonByIdQuery, LessonDetailResponse>
{
    public async ValueTask<Result<LessonDetailResponse>> Handle(
        GetLessonByIdQuery query, CancellationToken cancellationToken)
    {
        var course = await courses
            .Find(c => c.Id == query.CourseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result.Failure<LessonDetailResponse>(CourseErrors.NotFound(query.CourseId));

        var module = course.Modules.FirstOrDefault(m => m.ModuleId == query.ModuleId);

        if (module is null)
            return Result.Failure<LessonDetailResponse>(CourseErrors.ModuleNotFound(query.ModuleId));

        var lesson = module.Lessons.FirstOrDefault(l => l.LessonId == query.LessonId);

        if (lesson is null)
            return Result.Failure<LessonDetailResponse>(CourseErrors.LessonNotFound(query.LessonId));

        return new LessonDetailResponse(
            lesson.LessonId,
            lesson.Title,
            lesson.Type,
            lesson.Order,
            lesson.Content,
            lesson.Challenge);
    }
}
