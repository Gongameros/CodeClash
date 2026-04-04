namespace CodeClash.Courses.Tests.Infrastructure;

internal static class Ct
{
    internal static CancellationToken Token => TestContext.Current.CancellationToken;
}
