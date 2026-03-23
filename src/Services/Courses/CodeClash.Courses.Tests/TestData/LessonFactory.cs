using CodeClash.Courses.Features.Lessons.AddLesson;

namespace CodeClash.Courses.Tests.TestData;

public static class LessonFactory
{
    public static AddLessonCommand CreateCommand(
        string courseId,
        string moduleId,
        string authorId = "author-1",
        string title = "Test Lesson",
        LessonType type = LessonType.Theory,
        int order = 0,
        string? content = null,
        CodingChallenge? challenge = null) => new(
            courseId,
            moduleId,
            authorId,
            title,
            type,
            order,
            type == LessonType.Theory ? (content ?? "# Hello World") : null,
            challenge);
}
