using CodeClash.Web.ApiClients.Models.Enums;

namespace CodeClash.Web.ApiClients.Models.Responses;

public record CourseListItem(
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
