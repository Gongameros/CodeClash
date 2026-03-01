using CodeClash.Courses.Domains.Courses;
using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Courses.UpdateCourse;

public sealed record UpdateCourseCommand(
    string CourseId,
    string AuthorId,
    string? Title,
    string? Description,
    List<CodingTechnology>? CodingTechnologies,
    CourseDifficulty? Difficulty,
    List<string>? Tags,
    string? ThumbnailUrl,
    bool? IsPublished) : ICommand;