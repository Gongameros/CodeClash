using CodeClash.Courses.Domains.Courses;
using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Lessons.AddLesson;

public sealed record AddLessonCommand(
    string CourseId,
    string ModuleId,
    string AuthorId,
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallenge? Challenge) : ICommand<AddLessonResponse>;