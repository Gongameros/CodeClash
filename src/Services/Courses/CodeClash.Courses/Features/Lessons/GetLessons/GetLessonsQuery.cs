using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Lessons.GetLessons;

public sealed record GetLessonsQuery(string CourseId, string ModuleId)
    : IQuery<IReadOnlyList<LessonListItem>>;