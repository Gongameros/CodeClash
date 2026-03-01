using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Lessons.GetLessonById;

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