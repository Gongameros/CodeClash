using CodeClash.Web.ApiClients.Models.Enums;

namespace CodeClash.Web.ApiClients.Models.Requests;

public record CreateCourseRequest(
    string Title,
    string Description,
    List<CodingTechnology> CodingTechnologies,
    CourseDifficulty Difficulty,
    List<string> Tags,
    string? ThumbnailUrl);
