using MongoDB.Bson;

namespace CodeClash.MongoDB.Extensions;

public static class MongoHelper
{
    public static string RandomId() => ObjectId.GenerateNewId().ToString();
}
