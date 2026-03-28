using Shouldly;

namespace CodeClash.Results.Tests.Tests;

public sealed class ErrorTypeTests
{
    [Theory]
    [InlineData(ErrorType.None, 0)]
    [InlineData(ErrorType.Failure, 1)]
    [InlineData(ErrorType.Validation, 2)]
    [InlineData(ErrorType.NotFound, 3)]
    [InlineData(ErrorType.Conflict, 4)]
    [InlineData(ErrorType.Unauthorized, 5)]
    [InlineData(ErrorType.Forbidden, 6)]
    [InlineData(ErrorType.Unavailable, 7)]
    public void ErrorType_HasExpectedIntValue(ErrorType type, int expected)
    {
        ((int)type).ShouldBe(expected);
    }

    [Fact]
    public void ErrorType_HasEightMembers()
    {
        Enum.GetValues<ErrorType>().Length.ShouldBe(8);
    }
}
