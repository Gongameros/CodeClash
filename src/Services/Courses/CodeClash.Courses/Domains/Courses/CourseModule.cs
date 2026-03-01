using CodeClash.MongoDB.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CodeClash.Courses.Domains.Courses;

public class CourseModule
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string ModuleId { get; set; } = MongoHelper.RandomId();
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; }
    public int XpReward { get; set; }

    public List<Lesson> Lessons { get; set; } = [];
}
