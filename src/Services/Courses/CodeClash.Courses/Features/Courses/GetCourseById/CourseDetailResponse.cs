using CodeClash.Courses.Domains.Courses;

namespace CodeClash.Courses.Features.Courses.GetCourseById;

public sealed record CourseDetailResponse(
    string Id,
    string AuthorId,
    string Title,
    string Description,
    List<CodingTechnology> CodingTechnologies,
    CourseDifficulty Difficulty,
    List<string> Tags,
    string? ThumbnailUrl,
    int TotalXp,
    int EnrolledCount,
    double Rating,
    int RatingCount,
    List<ModuleResponse> Modules,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt);
