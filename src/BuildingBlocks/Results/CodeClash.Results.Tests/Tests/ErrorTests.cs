using Shouldly;

namespace CodeClash.Results.Tests.Tests;

public sealed class ErrorTests
{
    // ── Singletons ──────────────────────────────────────────────────

    [Fact]
    public void None_HasEmptyCodeAndDescription()
    {
        Error.None.Code.ShouldBe(string.Empty);
        Error.None.Description.ShouldBe(string.Empty);
        Error.None.Type.ShouldBe(ErrorType.None);
    }

    [Fact]
    public void NullValue_HasCorrectValues()
    {
        Error.NullValue.Code.ShouldBe("Error.NullValue");
        Error.NullValue.Description.ShouldBe("A null value was provided.");
        Error.NullValue.Type.ShouldBe(ErrorType.Failure);
    }

    // ── Factory methods ─────────────────────────────────────────────

    [Fact]
    public void Failure_CreatesErrorWithFailureType()
    {
        var error = Error.Failure("ERR_001", "Something failed");

        error.Code.ShouldBe("ERR_001");
        error.Description.ShouldBe("Something failed");
        error.Type.ShouldBe(ErrorType.Failure);
    }

    [Fact]
    public void Validation_CreatesErrorWithValidationType()
    {
        var error = Error.Validation("VAL_001", "Field is required");

        error.Code.ShouldBe("VAL_001");
        error.Description.ShouldBe("Field is required");
        error.Type.ShouldBe(ErrorType.Validation);
    }

    [Fact]
    public void NotFound_CreatesErrorWithNotFoundType()
    {
        var error = Error.NotFound("NF_001", "Entity not found");

        error.Code.ShouldBe("NF_001");
        error.Description.ShouldBe("Entity not found");
        error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public void Conflict_CreatesErrorWithConflictType()
    {
        var error = Error.Conflict("CON_001", "Already exists");

        error.Code.ShouldBe("CON_001");
        error.Description.ShouldBe("Already exists");
        error.Type.ShouldBe(ErrorType.Conflict);
    }

    [Fact]
    public void Unauthorized_CreatesErrorWithUnauthorizedType()
    {
        var error = Error.Unauthorized("AUTH_001", "Not authenticated");

        error.Code.ShouldBe("AUTH_001");
        error.Description.ShouldBe("Not authenticated");
        error.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact]
    public void Forbidden_CreatesErrorWithForbiddenType()
    {
        var error = Error.Forbidden("FORB_001", "Access denied");

        error.Code.ShouldBe("FORB_001");
        error.Description.ShouldBe("Access denied");
        error.Type.ShouldBe(ErrorType.Forbidden);
    }

    [Fact]
    public void Unavailable_CreatesErrorWithUnavailableType()
    {
        var error = Error.Unavailable("SVC_001", "Service down");

        error.Code.ShouldBe("SVC_001");
        error.Description.ShouldBe("Service down");
        error.Type.ShouldBe(ErrorType.Unavailable);
    }

    // ── Record equality ─────────────────────────────────────────────

    [Fact]
    public void TwoErrors_SameValues_AreEqual()
    {
        var a = Error.Failure("X", "Y");
        var b = Error.Failure("X", "Y");

        a.ShouldBe(b);
    }

    [Fact]
    public void TwoErrors_DifferentCodes_AreNotEqual()
    {
        var a = Error.Failure("A", "desc");
        var b = Error.Failure("B", "desc");

        a.ShouldNotBe(b);
    }

    [Fact]
    public void TwoErrors_DifferentTypes_AreNotEqual()
    {
        var a = Error.Failure("X", "desc");
        var b = Error.Validation("X", "desc");

        a.ShouldNotBe(b);
    }

    [Fact]
    public void None_Singletons_AreSameReference()
    {
        ReferenceEquals(Error.None, Error.None).ShouldBeTrue();
    }

    [Fact]
    public void NullValue_Singletons_AreSameReference()
    {
        ReferenceEquals(Error.NullValue, Error.NullValue).ShouldBeTrue();
    }
}
