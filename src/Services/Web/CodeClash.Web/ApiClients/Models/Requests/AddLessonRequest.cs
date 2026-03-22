using CodeClash.Web.ApiClients.Models.Enums;

namespace CodeClash.Web.ApiClients.Models.Requests;

public record AddLessonRequest(
    string Title,
    LessonType Type,
    int Order,
    string? Content,
    CodingChallengeRequest? Challenge);

public record CodingChallengeRequest(
    string Title,
    string Description,
    CourseDifficulty Difficulty,
    string Language,
    string StarterCode,
    string? SolutionCode,
    List<TestCaseRequest> TestCases,
    int TimeLimitMs,
    int MemoryLimitMb,
    int XpReward);

public record TestCaseRequest(
    string Input,
    string ExpectedOutput,
    bool IsHidden,
    string? Description);
