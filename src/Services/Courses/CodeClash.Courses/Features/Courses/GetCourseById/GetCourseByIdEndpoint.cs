using CodeClash.Courses.Domains.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Courses.GetCourseById;

public sealed class GetCourseByIdEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/courses/{courseId}", Handle)
            .WithName("GetCourseById")
            .WithTags("Courses");
    }

    private static async Task<IResult> Handle(
        string courseId,
        Mediator.IMediator mediator,
        CancellationToken cancellationToken)
    {
        return (await mediator.Send(new GetCourseByIdQuery(courseId), cancellationToken))
            .ToProblemDetails();
    }
}

public sealed record GetCourseByIdQuery(string CourseId) : IQuery<CourseDetailResponse>;

public sealed record CourseDetailResponse(
    string Id,
    string AuthorId,
    string Title,
    string Description,
    List<CodingTechnology> CodingTechnologies,
    CourseDifficulty Difficulty,
    List<string> Tags,
    string? ThumbnailUrl,
    int TotalXp,
    int EnrolledCount,
    double Rating,
    int RatingCount,
    List<ModuleResponse> Modules,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ModuleResponse(
    string ModuleId,
    string Title,
    string? Description,
    int Order,
    int XpReward,
    List<LessonResponse> Lessons);

public sealed record LessonResponse(
    string LessonId,
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallenge? Challenge);

public sealed class GetCourseByIdHandler(IMongoCollection<Course> courses)
    : IQueryHandler<GetCourseByIdQuery, CourseDetailResponse>
{
    public async ValueTask<Result<CourseDetailResponse>> Handle(
        GetCourseByIdQuery query, CancellationToken cancellationToken)
    {
        var course = await courses
            .Find(c => c.Id == query.CourseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result.Failure<CourseDetailResponse>(CourseErrors.NotFound(query.CourseId));

        var response = new CourseDetailResponse(
            course.Id,
            course.AuthorId,
            course.Title,
            course.Description,
            course.CodingTechnologies,
            course.Difficulty,
            course.Tags,
            course.ThumbnailUrl,
            course.TotalXp,
            course.EnrolledCount,
            course.Rating,
            course.RatingCount,
            course.Modules.Select(m => new ModuleResponse(
                m.ModuleId,
                m.Title,
                m.Description,
                m.Order,
                m.XpReward,
                m.Lessons.Select(l => new LessonResponse(
                    l.LessonId,
                    l.Title,
                    l.Type,
                    l.Order,
                    l.Content,
                    l.Challenge)).ToList())).ToList(),
            course.IsPublished,
            course.CreatedAt,
            course.UpdatedAt);

        return response;
    }
}
