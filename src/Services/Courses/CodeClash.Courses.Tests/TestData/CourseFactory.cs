using CodeClash.Courses.Features.Courses.CreateCourse;

namespace CodeClash.Courses.Tests.TestData;

public static class CourseFactory
{
    public static CreateCourseCommand CreateCommand(
        string authorId = "author-1",
        string title = "Test Course",
        string description = "A test course description.",
        CourseDifficulty difficulty = CourseDifficulty.Beginner,
        List<CodingTechnology>? technologies = null,
        List<string>? tags = null,
        string? thumbnailUrl = null) => new(
            authorId,
            title,
            description,
            technologies ?? [CodingTechnology.CSharp],
            difficulty,
            tags ?? ["test"],
            thumbnailUrl);
}
