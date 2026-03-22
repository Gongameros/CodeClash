using CodeClash.Web.ApiClients.Models.Enums;

namespace CodeClash.Web.ApiClients.Models.Responses;

public record CourseDetailResponse(
    string Id,
    string AuthorId,
    string Title,
    string Description,
    CourseDifficulty Difficulty,
    List<string> Tags,
    List<CodingTechnology> CodingTechnologies,
    string? ThumbnailUrl,
    int TotalXp,
    int EnrolledCount,
    double Rating,
    int RatingCount,
    bool IsPublished,
    List<ModuleResponse> Modules,
    DateTime CreatedAt,
    DateTime UpdatedAt);
