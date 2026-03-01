using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CodeClash.Courses.Domains.Courses;

public class Course
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string AuthorId { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<CodingTechnology> CodingTechnologies { get; set; } = [];
    public CourseDifficulty Difficulty { get; set; }

    public List<string> Tags { get; set; } = [];
    public string? ThumbnailUrl { get; set; }
    public int TotalXp { get; set; }
    public int EnrolledCount { get; set; }
    public double Rating { get; set; }
    public int RatingCount { get; set; }

    public List<CourseModule> Modules { get; set; } = [];

    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
