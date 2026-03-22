namespace CodeClash.Web.ApiClients.Models.Responses;

public record ModuleResponse(
    string ModuleId,
    string Title,
    string? Description,
    int Order,
    int XpReward,
    List<LessonResponse> Lessons);
