namespace CodeClash.Web.ApiClients.Models.Responses;

public record ModuleListItem(
    string ModuleId,
    string Title,
    string? Description,
    int Order,
    int XpReward,
    int LessonCount);
