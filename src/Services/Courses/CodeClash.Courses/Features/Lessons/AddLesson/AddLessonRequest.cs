using CodeClash.Courses.Domains.Courses;

namespace CodeClash.Courses.Features.Lessons.AddLesson;

public sealed record AddLessonRequest(
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallenge? Challenge);
