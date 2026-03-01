using CodeClash.MongoDB.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CodeClash.Courses.Domains.Courses;

public class CodingChallenge
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string ChallengeId { get; set; } = MongoHelper.RandomId();
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!; // Markdown with problem statement
    public CourseDifficulty Difficulty { get; set; }
    public string Language { get; set; } = null!;

    public string StarterCode { get; set; } = null!;
    public string? SolutionCode { get; set; }

    public List<TestCase> TestCases { get; set; } = [];
    public int TimeLimitMs { get; set; } = 5000;
    public int MemoryLimitMb { get; set; } = 256;
    public int XpReward { get; set; }
}
