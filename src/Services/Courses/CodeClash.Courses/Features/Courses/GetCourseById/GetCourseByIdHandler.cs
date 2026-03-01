using CodeClash.Courses.Domains.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Courses.GetCourseById;

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