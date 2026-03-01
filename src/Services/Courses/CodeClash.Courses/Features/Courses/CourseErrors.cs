using CodeClash.Results;

namespace CodeClash.Courses.Features.Courses;

public static class CourseErrors
{
    public static Error NotFound(string courseId) =>
        Error.NotFound("Course.NotFound", $"Course with ID '{courseId}' was not found.");

    public static Error ModuleNotFound(string moduleId) =>
        Error.NotFound("Module.NotFound", $"Module with ID '{moduleId}' was not found.");

    public static Error LessonNotFound(string lessonId) =>
        Error.NotFound("Lesson.NotFound", $"Lesson with ID '{lessonId}' was not found.");
}
