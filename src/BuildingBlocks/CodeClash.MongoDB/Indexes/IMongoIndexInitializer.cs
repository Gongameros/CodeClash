namespace CodeClash.MongoDB.Indexes;

public interface IIndexInitializer
{
    Task CreateIndexAsync(CancellationToken cancellationToken = default);
}
