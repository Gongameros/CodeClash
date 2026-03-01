namespace CodeClash.Courses.Features.Modules.UpdateModule;

public sealed record UpdateModuleRequest(
    string? Title,
    string? Description,
    int? Order,
    int? XpReward);
