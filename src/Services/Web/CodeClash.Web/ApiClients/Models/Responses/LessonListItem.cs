using CodeClash.Web.ApiClients.Models.Enums;

namespace CodeClash.Web.ApiClients.Models.Responses;

public record LessonListItem(
    string LessonId,
    string Title,
    LessonType Type,
    int Order);
