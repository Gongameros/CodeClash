using CodeClash.Courses.Domains.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Endpoints;
using CodeClash.Utilities.Messaging;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Courses.GetCourses;

public sealed class GetCoursesEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/courses", Handle)
            .WithName("GetCourses")
            .WithTags("Courses");
    }

    private static async Task<IResult> Handle(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        Mediator.IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        return (await mediator.Send(new GetCoursesQuery(page, pageSize), cancellationToken))
            .ToProblemDetails();
    }
}

public sealed record GetCoursesQuery(int Page = 1, int PageSize = 10)
    : IQuery<IReadOnlyList<CourseListItem>>;

public sealed record CourseListItem(
    string Id,
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
    bool IsPublished,
    DateTime CreatedAt);

public sealed class GetCoursesHandler(IMongoCollection<Course> courses)
    : IQueryHandler<GetCoursesQuery, IReadOnlyList<CourseListItem>>
{
    public async ValueTask<Result<IReadOnlyList<CourseListItem>>> Handle(
        GetCoursesQuery query, CancellationToken cancellationToken)
    {
        var items = await courses
            .Find(FilterDefinition<Course>.Empty)
            .SortByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
            .Project(c => new CourseListItem(
                c.Id,
                c.Title,
                c.Description,
                c.CodingTechnologies,
                c.Difficulty,
                c.Tags,
                c.ThumbnailUrl,
                c.TotalXp,
                c.EnrolledCount,
                c.Rating,
                c.RatingCount,
                c.IsPublished,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return items.AsReadOnly();
    }
}
