namespace CodeClash.Courses.Features.Modules.AddModule;

public sealed record AddModuleRequest(
    string Title,
    string? Description,
    int Order,
    int XpReward);
