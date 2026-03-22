using CodeClash.Web.ApiClients.Models.Enums;

namespace CodeClash.Web.ApiClients.Models.Responses;

public record LessonDetailResponse(
    string LessonId,
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallengeResponse? Challenge);

public record CodingChallengeResponse(
    string ChallengeId,
    string Title,
    string Description,
    CourseDifficulty Difficulty,
    string Language,
    string StarterCode,
    string? SolutionCode,
    List<TestCaseResponse> TestCases,
    int TimeLimitMs,
    int MemoryLimitMb,
    int XpReward);

public record TestCaseResponse(
    string Input,
    string ExpectedOutput,
    bool IsHidden,
    string? Description);
