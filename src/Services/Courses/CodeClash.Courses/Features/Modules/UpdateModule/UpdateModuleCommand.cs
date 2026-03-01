using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Modules.UpdateModule;

public sealed record UpdateModuleCommand(
    string CourseId,
    string ModuleId,
    string AuthorId,
    string? Title,
    string? Description,
    int? Order,
    int? XpReward) : ICommand;