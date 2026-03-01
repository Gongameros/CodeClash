namespace CodeClash.Courses.Features.Courses.GetCourseById;

public sealed record ModuleResponse(
    string ModuleId,
    string Title,
    string? Description,
    int Order,
    int XpReward,
    List<LessonResponse> Lessons);