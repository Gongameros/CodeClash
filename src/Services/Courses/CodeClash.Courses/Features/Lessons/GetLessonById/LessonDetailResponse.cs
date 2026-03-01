using CodeClash.Courses.Domains.Courses;

namespace CodeClash.Courses.Features.Lessons.GetLessonById;

public sealed record LessonDetailResponse(
    string LessonId,
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallenge? Challenge);
