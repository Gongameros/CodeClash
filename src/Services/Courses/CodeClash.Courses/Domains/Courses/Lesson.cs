using CodeClash.MongoDB.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CodeClash.Courses.Domains.Courses;

public class Lesson
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string LessonId { get; set; } = MongoHelper.RandomId();
    public string Title { get; set; } = null!;
    public LessonType Type { get; set; }
    public int Order { get; set; }

    // Theory content (markdown)
    public string? Content { get; set; }

    // Coding challenge
    public CodingChallenge? Challenge { get; set; }
}