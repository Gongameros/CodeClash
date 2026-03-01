using CodeClash.Courses.Domains.Courses;

namespace CodeClash.Courses.Features.Courses.UpdateCourse;

public sealed record UpdateCourseRequest(
    string? Title,
    string? Description,
    List<CodingTechnology>? CodingTechnologies,
    CourseDifficulty? Difficulty,
    List<string>? Tags,
    string? ThumbnailUrl,
    bool? IsPublished);
