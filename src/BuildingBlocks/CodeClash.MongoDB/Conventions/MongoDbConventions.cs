using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;

namespace CodeClash.MongoDB.Conventions;

public static class MongoDbConventions
{
    public static void RegisterConventions()
    {
        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new EnumRepresentationConvention(BsonType.String),
            new IgnoreIfNullConvention(true),
            new IgnoreExtraElementsConvention(true),
            new ImmutableTypeClassMapConvention()
        };

        ConventionRegistry.Register("CodeClashConventions", pack, _ => true);
    }
}
