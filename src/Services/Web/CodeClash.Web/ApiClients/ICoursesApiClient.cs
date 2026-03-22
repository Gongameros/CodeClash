using CodeClash.Web.ApiClients.Models;
using CodeClash.Web.ApiClients.Models.Requests;
using CodeClash.Web.ApiClients.Models.Responses;

namespace CodeClash.Web.ApiClients;

public interface ICoursesApiClient
{
    // Courses
    Task<PagedResult<CourseListItem>> GetCoursesAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<CourseDetailResponse?> GetCourseByIdAsync(string courseId, CancellationToken ct = default);
    Task<CreateCourseResponse?> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct = default);
    Task UpdateCourseAsync(string courseId, UpdateCourseRequest request, CancellationToken ct = default);
    Task DeleteCourseAsync(string courseId, CancellationToken ct = default);

    // Modules
    Task<List<ModuleListItem>> GetModulesAsync(string courseId, CancellationToken ct = default);
    Task<ModuleDetailResponse?> GetModuleByIdAsync(string courseId, string moduleId, CancellationToken ct = default);
    Task<AddModuleResponse?> AddModuleAsync(string courseId, AddModuleRequest request, CancellationToken ct = default);
    Task UpdateModuleAsync(string courseId, string moduleId, UpdateModuleRequest request, CancellationToken ct = default);
    Task DeleteModuleAsync(string courseId, string moduleId, CancellationToken ct = default);

    // Lessons
    Task<List<LessonListItem>> GetLessonsAsync(string courseId, string moduleId, CancellationToken ct = default);
    Task<LessonDetailResponse?> GetLessonByIdAsync(string courseId, string moduleId, string lessonId, CancellationToken ct = default);
    Task<AddLessonResponse?> AddLessonAsync(string courseId, string moduleId, AddLessonRequest request, CancellationToken ct = default);
    Task UpdateLessonAsync(string courseId, string moduleId, string lessonId, UpdateLessonRequest request, CancellationToken ct = default);
    Task DeleteLessonAsync(string courseId, string moduleId, string lessonId, CancellationToken ct = default);
}
