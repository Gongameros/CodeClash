using CodeClash.Courses.Domains.Courses;
using CodeClash.Courses.Features.Courses;
using CodeClash.Results;
using CodeClash.Utilities.Messaging;
using MongoDB.Driver;

namespace CodeClash.Courses.Features.Modules.GetModules;

public sealed class GetModulesHandler(IMongoCollection<Course> courses)
    : IQueryHandler<GetModulesQuery, IReadOnlyList<ModuleListItem>>
{
    public async ValueTask<Result<IReadOnlyList<ModuleListItem>>> Handle(
        GetModulesQuery query, CancellationToken cancellationToken)
    {
        var course = await courses
            .Find(c => c.Id == query.CourseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result.Failure<IReadOnlyList<ModuleListItem>>(CourseErrors.NotFound(query.CourseId));

        IReadOnlyList<ModuleListItem> modules = course.Modules
            .OrderBy(m => m.Order)
            .Select(m => new ModuleListItem(
                m.ModuleId,
                m.Title,
                m.Description,
                m.Order,
                m.XpReward,
                m.Lessons.Count))
            .ToList()
            .AsReadOnly();

        return Result.Success(modules);
    }
}