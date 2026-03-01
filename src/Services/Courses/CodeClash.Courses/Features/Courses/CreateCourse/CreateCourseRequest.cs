using CodeClash.Courses.Domains.Courses;

namespace CodeClash.Courses.Features.Courses.CreateCourse;

public sealed record CreateCourseRequest(
    string Title,
    string Description,
    List<CodingTechnology> CodingTechnologies,
    CourseDifficulty Difficulty,
    List<string> Tags,
    string? ThumbnailUrl);
