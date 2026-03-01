using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Modules.AddModule;

public sealed record AddModuleCommand(
    string CourseId,
    string AuthorId,
    string Title,
    string? Description,
    int Order,
    int XpReward) : ICommand<AddModuleResponse>;