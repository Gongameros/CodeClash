using CodeClash.Web.ApiClients.Models.Enums;

namespace CodeClash.Web.ApiClients.Models.Requests;

public record UpdateLessonRequest(
    string? Title,
    LessonType? Type,
    int? Order,
    string? Content,
    CodingChallengeRequest? Challenge);
