using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Modules.GetModules;

public sealed record GetModulesQuery(string CourseId) : IQuery<IReadOnlyList<ModuleListItem>>;