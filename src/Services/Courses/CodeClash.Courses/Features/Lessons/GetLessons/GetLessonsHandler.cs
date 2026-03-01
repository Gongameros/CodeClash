using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Lessons.GetLessons;

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