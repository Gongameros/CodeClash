using CodeClash.Web.ApiClients.Models.Enums;

namespace CodeClash.Web.ApiClients.Models.Responses;

public record LessonResponse(
    string LessonId,
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallengeResponse? Challenge);
