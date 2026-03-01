namespace CodeClash.Courses.Features.Modules.GetModuleById;

public sealed record ModuleDetailResponse(
    string ModuleId,
    string Title,
    string? Description,
    int Order,
    int XpReward,
    List<LessonListItem> Lessons);
