using CodeClash.Courses.Domains.Courses;
using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Lessons.UpdateLesson;

public sealed record UpdateLessonCommand(
    string CourseId,
    string ModuleId,
    string LessonId,
    string AuthorId,
    string? Title,
    LessonType? Type,
    int? Order,
    string? Content,
    CodingChallenge? Challenge) : ICommand;