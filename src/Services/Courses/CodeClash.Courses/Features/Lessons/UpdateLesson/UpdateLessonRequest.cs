using CodeClash.Courses.Domains.Courses;

namespace CodeClash.Courses.Features.Lessons.UpdateLesson;

public sealed record UpdateLessonRequest(
    string? Title,
    LessonType? Type,
    int? Order,
    string? Content,
    CodingChallenge? Challenge);
