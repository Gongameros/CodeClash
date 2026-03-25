namespace CodeClash.E2E.Tests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public class E2ECollection : ICollectionFixture<E2EFixture>
{
    public const string Name = "E2E";
}
