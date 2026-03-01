using CodeClash.Courses.Domains.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Courses.GetCourses;

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