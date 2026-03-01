namespace CodeClash.Courses.Features.Modules.GetModules;

public sealed record ModuleListItem(
    string ModuleId,
    string Title,
    string? Description,
    int Order,
    int XpReward,
    int LessonCount);
