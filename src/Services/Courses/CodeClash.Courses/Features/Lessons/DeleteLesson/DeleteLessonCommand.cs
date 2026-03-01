using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Lessons.DeleteLesson;

public sealed record DeleteLessonCommand(
    string CourseId,
    string ModuleId,
    string LessonId,
    string AuthorId) : ICommand;
