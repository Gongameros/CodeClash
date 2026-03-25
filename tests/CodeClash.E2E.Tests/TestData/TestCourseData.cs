namespace CodeClash.E2E.Tests.TestData;

public static class TestCourseData
{
    public static string UniqueTitle(string prefix = "E2E Test Course") =>
        $"{prefix} {Guid.NewGuid().ToString("N")[..8]}";

    public static CourseFormData Default(string? title = null) => new()
    {
        Title = title ?? UniqueTitle(),
        Description = "This is an automated E2E test course with detailed description for validation purposes.",
        Difficulty = "Beginner",
        Technologies = ["C#", "JavaScript"],
        Tags = ["e2e-test", "automation"]
    };

    public static CourseFormData Advanced(string? title = null) => new()
    {
        Title = title ?? UniqueTitle("Advanced Course"),
        Description = "An advanced course covering complex topics in software development and architecture.",
        Difficulty = "Advanced",
        Technologies = ["C#", "TypeScript", "Python"],
        Tags = ["advanced", "architecture", "patterns"]
    };

    public static ModuleFormData DefaultModule(string? title = null) => new()
    {
        Title = title ?? $"Test Module {Guid.NewGuid().ToString()[..8]}",
        Description = "An E2E test module with description.",
        Order = 0,
        XpReward = 100
    };

    public static LessonFormData DefaultLesson(string? title = null) => new()
    {
        Title = title ?? $"Test Lesson {Guid.NewGuid().ToString()[..8]}",
        Type = "Theory",
        Order = 0,
        Content = "# Test Lesson\n\nThis is test lesson content in **Markdown**."
    };
}

public class CourseFormData
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Beginner";
    public List<string> Technologies { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public bool Publish { get; set; }
}

public class ModuleFormData
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public int XpReward { get; set; } = 100;
}

public class LessonFormData
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "Theory";
    public int Order { get; set; }
    public string Content { get; set; } = string.Empty;
}
