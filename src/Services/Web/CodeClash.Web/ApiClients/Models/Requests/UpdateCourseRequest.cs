using CodeClash.Web.ApiClients.Models.Enums;

namespace CodeClash.Web.ApiClients.Models.Requests;

public record UpdateCourseRequest(
    string? Title,
    string? Description,
    List<CodingTechnology>? CodingTechnologies,
    CourseDifficulty? Difficulty,
    List<string>? Tags,
    string? ThumbnailUrl,
    bool? IsPublished);
