using CodeClash.Courses.Domains.Courses;

namespace CodeClash.Courses.Features.Courses.GetCourseById;

public sealed record LessonResponse(
    string LessonId,
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallenge? Challenge);