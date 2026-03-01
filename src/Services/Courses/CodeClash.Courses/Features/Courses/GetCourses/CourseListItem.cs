using CodeClash.Courses.Domains.Courses;

namespace CodeClash.Courses.Features.Courses.GetCourses;

public sealed record CourseListItem(
    string Id,
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
    bool IsPublished,
    DateTime CreatedAt);
