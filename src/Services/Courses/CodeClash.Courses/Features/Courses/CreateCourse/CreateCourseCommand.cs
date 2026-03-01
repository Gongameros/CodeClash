using CodeClash.Courses.Domains.Courses;
using CodeClash.Utilities.Messaging;

namespace CodeClash.Courses.Features.Courses.CreateCourse;

public sealed record CreateCourseCommand(
    string AuthorId,
    string Title,
    string Description,
    List<CodingTechnology> CodingTechnologies,
    CourseDifficulty Difficulty,
    List<string> Tags,
    string? ThumbnailUrl) : ICommand<CreateCourseResponse>;