using CodeClash.Courses.Features.Modules.AddModule;

namespace CodeClash.Courses.Tests.TestData;

public static class ModuleFactory
{
    public static AddModuleCommand CreateCommand(
        string courseId,
        string authorId = "author-1",
        string title = "Test Module",
        string? description = "Module description",
        int order = 0,
        int xpReward = 100) => new(courseId, authorId, title, description, order, xpReward);
}
