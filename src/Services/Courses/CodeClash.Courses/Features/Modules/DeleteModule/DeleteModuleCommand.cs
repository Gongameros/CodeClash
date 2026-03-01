using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Modules.DeleteModule;

public sealed record DeleteModuleCommand(string CourseId, string ModuleId, string AuthorId)
    : ICommand;
