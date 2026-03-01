using CodeClash.Courses.Domains.Courses;

namespace CodeClash.Courses.Features.Modules.GetModuleById;

public sealed record LessonListItem(
    string LessonId,
    string Title,
    LessonType Type,
    int Order);