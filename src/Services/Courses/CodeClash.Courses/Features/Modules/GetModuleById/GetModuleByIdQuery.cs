using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Modules.GetModuleById;

public sealed record GetModuleByIdQuery(string CourseId, string ModuleId)
    : IQuery<ModuleDetailResponse>;