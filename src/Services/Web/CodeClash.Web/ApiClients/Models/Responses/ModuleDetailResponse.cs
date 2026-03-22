namespace CodeClash.Web.ApiClients.Models.Responses;

public record ModuleDetailResponse(
    string ModuleId,
    string Title,
    string? Description,
    int Order,
    int XpReward,
    List<LessonListItem> Lessons);
