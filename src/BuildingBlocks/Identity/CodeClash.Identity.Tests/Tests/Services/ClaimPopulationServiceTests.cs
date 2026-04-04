using System.Security.Claims;
using CodeClash.Identity.Attributes;
using CodeClash.Identity.Services;
using CodeClash.Identity.Tests.Helpers;
using Shouldly;

namespace CodeClash.Identity.Tests.Tests.Services;

public sealed class ClaimPopulationServiceTests
{
    private readonly ClaimPopulationService _sut = new();

    // --- Test model classes ---

    private class SimpleCommand
    {
        [FromUserId]
        public string UserId { get; set; } = default!;
    }

    private class CommandWithExplicitClaim
    {
        [FromClaim("preferred_username")]
        public string Username { get; set; } = default!;
    }

    private class CommandWithAlternateClaim
    {
        [FromClaim(ClaimType = "primary_id", AlternateClaimType = "sub")]
        public string Id { get; set; } = default!;
    }

    private class CommandWithOptionalClaim
    {
        [FromClaim("nickname", Required = false)]
        public string? Nickname { get; set; }
    }

    private class CommandWithGuidProperty
    {
        [FromUserId]
        public Guid UserId { get; set; }
    }

    private class CommandWithIntProperty
    {
        [FromClaim("age")]
        public int Age { get; set; }
    }

    private class CommandWithLongProperty
    {
        [FromClaim("timestamp")]
        public long Timestamp { get; set; }
    }

    private class CommandWithBoolProperty
    {
        [FromClaim("is_admin")]
        public bool IsAdmin { get; set; }
    }

    private class CommandWithDateTimeProperty
    {
        [FromClaim("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    private class CommandWithDateTimeOffsetProperty
    {
        [FromClaim("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    private class CommandWithNullableGuid
    {
        [FromClaim("optional_id", Required = false)]
        public Guid? OptionalId { get; set; }
    }

    private class CommandWithNullableInt
    {
        [FromClaim("score", Required = false)]
        public int? Score { get; set; }
    }

    private class CommandWithNoAttributes
    {
        public string Name { get; set; } = "original";
    }

    private class CommandWithMultipleAttributes
    {
        [FromUserId]
        public string UserId { get; set; } = default!;

        [FromClaim("preferred_username")]
        public string Username { get; set; } = default!;

        [FromClaim("email")]
        public string Email { get; set; } = default!;
    }

    private class CommandWithReadOnlyProperty
    {
        [FromClaim("value")]
        public string ReadOnly { get; } = "immutable";
    }

    private class CommandWithPropertyNameAsClaim
    {
        [FromClaim]
        public string email { get; set; } = default!;
    }

    // --- Null / unauthenticated ---

    [Fact]
    public void PopulateFromClaims_NullObject_ThrowsArgumentNullException()
    {
        var user = ClaimsPrincipalFactory.CreateAuthenticated();

        Should.Throw<ArgumentNullException>(() => _sut.PopulateFromClaims(null!, user));
    }

    [Fact]
    public void PopulateFromClaims_UnauthenticatedUser_DoesNotPopulate()
    {
        var command = new SimpleCommand();
        var user = ClaimsPrincipalFactory.CreateUnauthenticated();

        _sut.PopulateFromClaims(command, user);

        command.UserId.ShouldBe(default!);
    }

    // --- Basic claim population ---

    [Fact]
    public void PopulateFromClaims_WithNameIdentifier_PopulatesUserId()
    {
        var command = new SimpleCommand();
        var user = ClaimsPrincipalFactory.CreateWithUserId("user-123");

        _sut.PopulateFromClaims(command, user);

        command.UserId.ShouldBe("user-123");
    }

    [Fact]
    public void PopulateFromClaims_WithSubClaim_PopulatesViaAlternate()
    {
        var command = new SimpleCommand();
        var user = ClaimsPrincipalFactory.CreateWithSubClaim("sub-456");

        _sut.PopulateFromClaims(command, user);

        command.UserId.ShouldBe("sub-456");
    }

    [Fact]
    public void PopulateFromClaims_ExplicitClaimType_PopulatesCorrectly()
    {
        var command = new CommandWithExplicitClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("preferred_username", "johndoe"));

        _sut.PopulateFromClaims(command, user);

        command.Username.ShouldBe("johndoe");
    }

    // --- Alternate claim ---

    [Fact]
    public void PopulateFromClaims_PrimaryMissing_UsesAlternateClaim()
    {
        var command = new CommandWithAlternateClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("sub", "alt-id-789"));

        _sut.PopulateFromClaims(command, user);

        command.Id.ShouldBe("alt-id-789");
    }

    [Fact]
    public void PopulateFromClaims_PrimaryPresent_DoesNotUseAlternate()
    {
        var command = new CommandWithAlternateClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("primary_id", "primary-val"),
            new Claim("sub", "alt-val"));

        _sut.PopulateFromClaims(command, user);

        command.Id.ShouldBe("primary-val");
    }

    // --- Optional claims ---

    [Fact]
    public void PopulateFromClaims_OptionalClaimMissing_DoesNotThrow()
    {
        var command = new CommandWithOptionalClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated();

        Should.NotThrow(() => _sut.PopulateFromClaims(command, user));
        command.Nickname.ShouldBeNull();
    }

    [Fact]
    public void PopulateFromClaims_OptionalClaimPresent_Populates()
    {
        var command = new CommandWithOptionalClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("nickname", "Johnny"));

        _sut.PopulateFromClaims(command, user);

        command.Nickname.ShouldBe("Johnny");
    }

    // --- Required claim missing ---

    [Fact]
    public void PopulateFromClaims_RequiredClaimMissing_ThrowsUnauthorizedAccess()
    {
        var command = new SimpleCommand();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(); // no NameIdentifier

        var ex = Should.Throw<UnauthorizedAccessException>(
            () => _sut.PopulateFromClaims(command, user));

        ex.Message.ShouldContain("Required claim");
        ex.Message.ShouldContain("UserId");
    }

    // --- Type conversions ---

    [Fact]
    public void PopulateFromClaims_GuidProperty_ConvertsCorrectly()
    {
        var command = new CommandWithGuidProperty();
        var guid = Guid.NewGuid();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.NameIdentifier, guid.ToString()));

        _sut.PopulateFromClaims(command, user);

        command.UserId.ShouldBe(guid);
    }

    [Fact]
    public void PopulateFromClaims_IntProperty_ConvertsCorrectly()
    {
        var command = new CommandWithIntProperty();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("age", "25"));

        _sut.PopulateFromClaims(command, user);

        command.Age.ShouldBe(25);
    }

    [Fact]
    public void PopulateFromClaims_LongProperty_ConvertsCorrectly()
    {
        var command = new CommandWithLongProperty();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("timestamp", "1234567890123"));

        _sut.PopulateFromClaims(command, user);

        command.Timestamp.ShouldBe(1234567890123L);
    }

    [Fact]
    public void PopulateFromClaims_BoolProperty_ConvertsCorrectly()
    {
        var command = new CommandWithBoolProperty();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("is_admin", "true"));

        _sut.PopulateFromClaims(command, user);

        command.IsAdmin.ShouldBeTrue();
    }

    [Fact]
    public void PopulateFromClaims_DateTimeProperty_ConvertsCorrectly()
    {
        var command = new CommandWithDateTimeProperty();
        var dateString = "2025-06-15T10:30:00";
        var expected = DateTime.Parse(dateString);
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("created_at", dateString));

        _sut.PopulateFromClaims(command, user);

        command.CreatedAt.ShouldBe(expected);
    }

    [Fact]
    public void PopulateFromClaims_DateTimeOffsetProperty_ConvertsCorrectly()
    {
        var command = new CommandWithDateTimeOffsetProperty();
        var date = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero);
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("updated_at", date.ToString("O")));

        _sut.PopulateFromClaims(command, user);

        command.UpdatedAt.ShouldBe(date);
    }

    [Fact]
    public void PopulateFromClaims_NullableGuid_WithValue_ConvertsCorrectly()
    {
        var command = new CommandWithNullableGuid();
        var guid = Guid.NewGuid();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("optional_id", guid.ToString()));

        _sut.PopulateFromClaims(command, user);

        command.OptionalId.ShouldBe(guid);
    }

    [Fact]
    public void PopulateFromClaims_NullableInt_WithValue_ConvertsCorrectly()
    {
        var command = new CommandWithNullableInt();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("score", "100"));

        _sut.PopulateFromClaims(command, user);

        command.Score.ShouldBe(100);
    }

    [Fact]
    public void PopulateFromClaims_NullableGuid_Missing_RemainsNull()
    {
        var command = new CommandWithNullableGuid();
        var user = ClaimsPrincipalFactory.CreateAuthenticated();

        _sut.PopulateFromClaims(command, user);

        command.OptionalId.ShouldBeNull();
    }

    // --- No attributes ---

    [Fact]
    public void PopulateFromClaims_NoAttributes_DoesNotModifyObject()
    {
        var command = new CommandWithNoAttributes();
        var user = ClaimsPrincipalFactory.CreateFullUser();

        _sut.PopulateFromClaims(command, user);

        command.Name.ShouldBe("original");
    }

    // --- Multiple attributes ---

    [Fact]
    public void PopulateFromClaims_MultipleAttributes_PopulatesAll()
    {
        var command = new CommandWithMultipleAttributes();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim(ClaimTypes.NameIdentifier, "uid-1"),
            new Claim("preferred_username", "johndoe"),
            new Claim("email", "john@test.com"));

        _sut.PopulateFromClaims(command, user);

        command.UserId.ShouldBe("uid-1");
        command.Username.ShouldBe("johndoe");
        command.Email.ShouldBe("john@test.com");
    }

    // --- Read-only property ---

    [Fact]
    public void PopulateFromClaims_ReadOnlyProperty_DoesNotThrow()
    {
        var command = new CommandWithReadOnlyProperty();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("value", "new-val"));

        Should.NotThrow(() => _sut.PopulateFromClaims(command, user));
        command.ReadOnly.ShouldBe("immutable");
    }

    // --- Property name as claim type fallback ---

    [Fact]
    public void PopulateFromClaims_NoExplicitClaimType_UsesPropertyName()
    {
        var command = new CommandWithPropertyNameAsClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("email", "test@example.com"));

        _sut.PopulateFromClaims(command, user);

        command.email.ShouldBe("test@example.com");
    }

    // --- Interface ---

    [Fact]
    public void ImplementsIClaimPopulationService()
    {
        _sut.ShouldBeAssignableTo<IClaimPopulationService>();
    }

    // --- Empty claim value ---

    [Fact]
    public void PopulateFromClaims_EmptyClaimValue_SetsNull()
    {
        var command = new CommandWithOptionalClaim();
        var user = ClaimsPrincipalFactory.CreateAuthenticated(
            new Claim("nickname", ""));

        _sut.PopulateFromClaims(command, user);

        // ConvertClaimValue returns null for empty string
        command.Nickname.ShouldBeNull();
    }
}
